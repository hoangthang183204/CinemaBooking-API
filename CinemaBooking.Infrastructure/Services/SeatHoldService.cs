using CinemaBooking.Application.Common;
using CinemaBooking.Application.DTO;
using CinemaBooking.Application.Interfaces;
using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Enums;
using CinemaBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Infrastructure.Services
{
    public class SeatHoldService : ISeatHoldService
    {
        public const int HoldMinutes = 10;
        public const int MaxSeatsPerHold = 8;

        private readonly CinemaDbContext _context;

        public SeatHoldService(CinemaDbContext context)
        {
            _context = context;
        }

        public SeatHoldResult Hold(int showTimeId, int userId, IReadOnlyCollection<int> seatIds)
        {
            var ids = seatIds.Distinct().ToList();
            if (ids.Count == 0)
                return SeatHoldResult.Fail("Chọn ít nhất một ghế.");
            if (ids.Count > MaxSeatsPerHold)
                return SeatHoldResult.Fail($"Tối đa {MaxSeatsPerHold} ghế mỗi lần giữ.");

            var showTime = _context.ShowTimes.FirstOrDefault(s => s.Id == showTimeId);
            if (showTime == null)
                return SeatHoldResult.Fail("Suất chiếu không tồn tại.");
            if (ShowTimeClock.HasStarted(showTime.StartTime))
                return SeatHoldResult.Fail("Suất chiếu đã qua giờ, không thể đặt vé.");

            var roomSeatIds = _context.Seats
                .Where(s => s.RoomId == showTime.RoomId)
                .Select(s => s.Id)
                .ToHashSet();

            var invalid = ids.Where(id => !roomSeatIds.Contains(id)).ToList();
            if (invalid.Count > 0)
                return SeatHoldResult.Fail($"Ghế không thuộc phòng này: {string.Join(", ", invalid)}.");

            var locked = _context.Seats.Where(s => ids.Contains(s.Id) && s.IsBlocked).Select(s => s.SeatNumber).ToList();
            if (locked.Count > 0)
                return SeatHoldResult.Fail($"Ghế đã khóa: {string.Join(", ", locked)}.");

            var now = DateTime.UtcNow;
            ReleaseExpired(showTimeId, now);

            var booked = _context.Tickets
                .Where(t => t.ShowTimeId == showTimeId && ids.Contains(t.SeatId))
                .Select(t => t.SeatId)
                .ToHashSet();
            var openBookingSeats = OpenBookingSeatIds(showTimeId, ids);
            booked.UnionWith(openBookingSeats);
            if (booked.Count > 0)
                return SeatHoldResult.Fail($"Ghế đã được đặt: {string.Join(", ", booked)}.");

            var activeHolds = _context.SeatHolds
                .Where(h => h.ShowTimeId == showTimeId && ids.Contains(h.SeatId) && h.ExpiresAt > now)
                .ToList();

            var takenByOther = activeHolds.Where(h => h.UserId != userId).Select(h => h.SeatId).ToList();
            if (takenByOther.Count > 0)
                return SeatHoldResult.Fail($"Ghế đang được người khác giữ: {string.Join(", ", takenByOther)}.");

            var token = Guid.NewGuid();
            var expiresAt = now.AddMinutes(HoldMinutes);
            var owned = activeHolds.ToDictionary(h => h.SeatId);

            foreach (var seatId in ids)
            {
                if (owned.TryGetValue(seatId, out var existing))
                {
                    existing.HoldToken = token;
                    existing.ExpiresAt = expiresAt;
                    continue;
                }

                _context.SeatHolds.Add(new SeatHold
                {
                    ShowTimeId = showTimeId,
                    SeatId = seatId,
                    UserId = userId,
                    HoldToken = token,
                    ExpiresAt = expiresAt,
                    CreatedAt = now
                });
            }

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateException)
            {
                // Unique (ShowTimeId, SeatId): hai request giữ cùng ghế cùng lúc.
                return SeatHoldResult.Fail("Ghế vừa được người khác giữ. Chọn ghế khác.");
            }

            return SeatHoldResult.Ok(new SeatHoldCreatedDto
            {
                HoldId = token,
                ExpiresAt = expiresAt,
                SeatIds = ids
            });
        }

        public bool Release(Guid holdToken, int userId)
        {
            var rows = _context.SeatHolds
                .Where(h => h.HoldToken == holdToken && h.UserId == userId)
                .ToList();
            if (rows.Count == 0)
                return false;

            _context.SeatHolds.RemoveRange(rows);
            _context.SaveChanges();
            return true;
        }

        public SeatAvailability GetAvailability(int showTimeId, int? userId)
        {
            var now = DateTime.UtcNow;
            var holds = _context.SeatHolds
                .Where(h => h.ShowTimeId == showTimeId && h.ExpiresAt > now)
                .Select(h => new { h.SeatId, h.UserId })
                .ToList();

            var openBookings = _context.BookingSeats
                .Where(bs => bs.Booking.ShowTimeId == showTimeId
                    && (bs.Booking.Status == BookingStatus.Pending || bs.Booking.Status == BookingStatus.Paid))
                .Select(bs => bs.SeatId)
                .ToHashSet();

            return new SeatAvailability
            {
                OpenBookingSeatIds = openBookings,
                HeldByOthers = holds.Where(h => h.UserId != userId).Select(h => h.SeatId).ToHashSet(),
                HeldByUser = holds.Where(h => userId != null && h.UserId == userId).Select(h => h.SeatId).ToHashSet()
            };
        }

        private HashSet<int> OpenBookingSeatIds(int showTimeId, IReadOnlyCollection<int> seatIds)
        {
            return _context.BookingSeats
                .Where(bs => bs.Booking.ShowTimeId == showTimeId
                    && seatIds.Contains(bs.SeatId)
                    && (bs.Booking.Status == BookingStatus.Pending || bs.Booking.Status == BookingStatus.Paid))
                .Select(bs => bs.SeatId)
                .ToHashSet();
        }

        private void ReleaseExpired(int showTimeId, DateTime now)
        {
            var expired = _context.SeatHolds
                .Where(h => h.ShowTimeId == showTimeId && h.ExpiresAt <= now)
                .ToList();
            if (expired.Count == 0)
                return;

            _context.SeatHolds.RemoveRange(expired);
            _context.SaveChanges();
        }
    }
}
