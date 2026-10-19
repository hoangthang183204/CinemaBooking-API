using CinemaBooking.Application.Common;
using CinemaBooking.Application.DTO;
using CinemaBooking.Application.Interfaces;
using CinemaBooking.Application.UseCases.CreateBooking;
using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Enums;
using CinemaBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Infrastructure.Services
{
    public class CreateBookingService : ICreateBookingService
    {
        private readonly CinemaDbContext _context;

        public CreateBookingService(CinemaDbContext context)
        {
            _context = context;
        }

        public CreateBookingResult Execute(CreateBookingRequest request)
        {
            var showtimeId = request.ShowtimeId;
            var seatIds = request.SeatIds?.Distinct().ToList() ?? new List<int>();

            if (seatIds.Count == 0)
                return CreateBookingResult.Fail("Chọn ít nhất một ghế.");
            if (seatIds.Count > SeatHoldService.MaxSeatsPerHold)
                return CreateBookingResult.Fail($"Tối đa {SeatHoldService.MaxSeatsPerHold} ghế mỗi đơn.");

            var showTime = _context.ShowTimes.FirstOrDefault(s => s.Id == showtimeId);
            if (showTime == null)
                return CreateBookingResult.Fail("Suất chiếu không tồn tại.");
            if (ShowTimeClock.HasStarted(showTime.StartTime))
                return CreateBookingResult.Fail("Suất chiếu đã qua giờ, không thể đặt vé.");

            if (request.UserId is int userId && userId > 0)
            {
                var existingId = _context.Bookings
                    .Where(b => b.UserId == userId
                        && (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Paid)
                        && (b.ShowTimeId == showtimeId
                            || (b.ShowTime.RoomId == showTime.RoomId
                                && b.ShowTime.MovieId == showTime.MovieId
                                && b.ShowTime.StartTime == showTime.StartTime)))
                    .Select(b => (int?)b.Id)
                    .FirstOrDefault();
                if (existingId != null)
                    return CreateBookingResult.Fail(
                        $"Bạn đã có đơn #{existingId} cho suất này, cùng phim, cùng phòng và cùng giờ. Hãy thanh toán hoặc hủy đơn đó trước khi đặt thêm.");
            }

            var blockedIds = _context.Seats
                .Where(s => s.RoomId == showTime.RoomId && s.IsBlocked)
                .Select(s => s.Id)
                .ToHashSet();
            if (seatIds.Any(id => blockedIds.Contains(id)))
                return CreateBookingResult.Fail("Có ghế đã khóa, không bán được.");

            var roomSeatIds = _context.Seats
                .Where(s => s.RoomId == showTime.RoomId)
                .Select(s => s.Id)
                .ToHashSet();

            var invalidSeats = seatIds.Where(id => !roomSeatIds.Contains(id)).ToList();
            if (invalidSeats.Count > 0)
                return CreateBookingResult.Fail($"Ghế không hợp lệ: {string.Join(", ", invalidSeats)}.");

            var bookedSeatIds = _context.Tickets
                .Where(t => t.ShowTimeId == showtimeId)
                .Select(t => t.SeatId)
                .ToHashSet();

            var openBookingSeatIds = _context.BookingSeats
                .Where(bs => bs.Booking.ShowTimeId == showtimeId
                    && seatIds.Contains(bs.SeatId)
                    && (bs.Booking.Status == BookingStatus.Pending || bs.Booking.Status == BookingStatus.Paid))
                .Select(bs => bs.SeatId)
                .ToHashSet();
            bookedSeatIds.UnionWith(openBookingSeatIds);

            var alreadyBooked = seatIds.Where(id => bookedSeatIds.Contains(id)).ToList();
            if (alreadyBooked.Count > 0)
                return CreateBookingResult.Fail($"Ghế đã được đặt: {string.Join(", ", alreadyBooked)}.");

            var now = DateTime.UtcNow;
            var holds = _context.SeatHolds
                .Where(h => h.ShowTimeId == showtimeId && seatIds.Contains(h.SeatId))
                .ToList();
            var blocked = holds
                .Where(h => h.ExpiresAt > now && h.UserId != request.UserId)
                .Select(h => h.SeatId)
                .ToList();
            if (blocked.Count > 0)
                return CreateBookingResult.Fail($"Ghế đang được người khác giữ: {string.Join(", ", blocked)}.");

            var picks = (request.Concessions ?? new List<ConcessionPickDto>())
                .Where(c => c.Quantity > 0)
                .GroupBy(c => c.ConcessionId)
                .Select(g => new ConcessionPickDto { ConcessionId = g.Key, Quantity = g.Sum(x => x.Quantity) })
                .ToList();
            if (picks.Any(c => c.Quantity > 8))
                return CreateBookingResult.Fail("Tối đa 8 phần cho mỗi món bắp nước.");

            var itemIds = picks.Select(c => c.ConcessionId).ToList();
            var menu = itemIds.Count == 0
                ? new List<ConcessionItem>()
                : _context.ConcessionItems.Where(c => itemIds.Contains(c.Id) && c.IsActive).ToList();
            if (menu.Count != itemIds.Count)
                return CreateBookingResult.Fail("Có món bắp nước không còn bán.");

            var concessionLines = new List<BookingConcession>();
            decimal snackTotal = 0;
            foreach (var pick in picks)
            {
                var item = menu.First(c => c.Id == pick.ConcessionId);
                if (item.StockQuantity < pick.Quantity)
                    return CreateBookingResult.Fail($"Không đủ {item.Name}. Còn {item.StockQuantity}.");
                item.StockQuantity -= pick.Quantity;
                snackTotal += item.Price * pick.Quantity;
                concessionLines.Add(new BookingConcession
                {
                    ConcessionItemId = item.Id,
                    ItemName = item.Name,
                    Quantity = pick.Quantity,
                    UnitPrice = item.Price
                });
            }

            var quote = CinemaPricing.Quote(_context, showtimeId, seatIds.Count, snackTotal, request.PromoCode, false, request.UserId);
            if (!string.IsNullOrWhiteSpace(request.PromoCode) && !quote.Ok)
                return CreateBookingResult.Fail(quote.Message);

            if (quote.Discount > 0 && quote.Code != null)
            {
                var used = _context.Promotions.FirstOrDefault(p => p.Code == quote.Code);
                if (used != null) used.UsedCount += 1;
            }

            var seatPrice = quote.SeatPrice;
            var totalPrice = quote.Total;
            var booking = new Booking
            {
                UserId = request.UserId,
                ShowTimeId = showtimeId,
                TotalPrice = totalPrice,
                DiscountAmount = quote.Discount,
                SurchargeAmount = quote.Surcharge,
                PromoCode = quote.Code,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var seatId in seatIds)
            {
                booking.BookingSeats.Add(new BookingSeat
                {
                    SeatId = seatId,
                    Price = seatPrice
                });
            }

            foreach (var line in concessionLines)
                booking.Concessions.Add(line);

            _context.Bookings.Add(booking);
            if (holds.Count > 0)
                _context.SeatHolds.RemoveRange(holds);

            foreach (var seatId in seatIds)
            {
                _context.Tickets.Add(new Ticket
                {
                    ShowTimeId = showtimeId,
                    SeatId = seatId,
                    Price = seatPrice
                });
            }

            _context.SaveChanges();

            return CreateBookingResult.Ok(new CreateBookingResponse
            {
                BookingId = booking.Id,
                TotalPrice = totalPrice,
                Message = "Đặt vé thành công."
            });
        }
    }
}
