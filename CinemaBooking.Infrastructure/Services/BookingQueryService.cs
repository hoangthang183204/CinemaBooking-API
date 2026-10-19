using CinemaBooking.Application.DTO;
using CinemaBooking.Application.Interfaces;
using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Enums;
using CinemaBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Infrastructure.Services
{
    public class BookingQueryService : IBookingQueryService
    {
        private readonly CinemaDbContext _context;

        public BookingQueryService(CinemaDbContext context)
        {
            _context = context;
        }

        public BookingDetailDto? GetById(int bookingId)
        {
            var b = _context.Bookings
                .Include(x => x.ShowTime).ThenInclude(s => s.Movie)
                .Include(x => x.ShowTime).ThenInclude(s => s.Room)
                .Include(x => x.BookingSeats).ThenInclude(bs => bs.Seat)
                .Include(x => x.Concessions)
                .Include(x => x.ElectronicTickets).ThenInclude(et => et.Seat)
                .FirstOrDefault(x => x.Id == bookingId);
            if (b == null) return null;

            var seatNumbers = b.BookingSeats?.Select(bs => bs.Seat?.SeatNumber ?? "").Where(s => !string.IsNullOrEmpty(s)).ToList() ?? new List<string>();
            var tickets = b.ElectronicTickets?.Select(et => new ElectronicTicketDto
            {
                Id = et.Id,
                SeatId = et.SeatId,
                SeatNumber = et.Seat?.SeatNumber ?? "",
                QRCode = et.QRCode
            }).ToList() ?? new List<ElectronicTicketDto>();

            var snackSubtotal = (b.Concessions ?? new List<BookingConcession>()).Sum(c => c.UnitPrice * c.Quantity);
            var ticketSubtotal = (b.BookingSeats ?? new List<BookingSeat>()).Sum(bs => bs.Price);
            if (ticketSubtotal <= 0)
                ticketSubtotal = b.TotalPrice + b.DiscountAmount - b.SurchargeAmount - snackSubtotal;

            return new BookingDetailDto
            {
                Id = b.Id,
                ShowTimeId = b.ShowTimeId,
                TotalPrice = b.TotalPrice,
                TicketSubtotal = ticketSubtotal,
                SnackSubtotal = snackSubtotal,
                SurchargeAmount = b.SurchargeAmount,
                DiscountAmount = b.DiscountAmount,
                PromoCode = b.PromoCode,
                Status = b.Status.ToString(),
                CreatedAt = b.CreatedAt,
                MovieTitle = b.ShowTime?.Movie?.Title ?? "",
                RoomName = b.ShowTime?.Room?.Name ?? "",
                ShowTimeStart = b.ShowTime?.StartTime ?? default,
                SeatNumbers = seatNumbers,
                Concessions = (b.Concessions ?? new List<BookingConcession>()).Select(c => new BookingConcessionLineDto
                {
                    Name = c.ItemName,
                    Quantity = c.Quantity,
                    UnitPrice = c.UnitPrice
                }).ToList(),
                Tickets = tickets
            };
        }

        public List<BookingSummaryDto> GetBookings(int? userId = null)
        {
            var query = _context.Bookings
                .Include(b => b.ShowTime).ThenInclude(s => s.Movie)
                .Include(b => b.ShowTime).ThenInclude(s => s.Room)
                .Include(b => b.BookingSeats).ThenInclude(bs => bs.Seat)
                .OrderByDescending(b => b.CreatedAt)
                .AsQueryable();
            if (userId.HasValue)
                query = query.Where(b => b.UserId == userId.Value);

            return query.ToList().Select(b => new BookingSummaryDto
            {
                Id = b.Id,
                MovieTitle = b.ShowTime?.Movie?.Title ?? "",
                RoomName = b.ShowTime?.Room?.Name ?? "",
                ShowTimeStart = b.ShowTime?.StartTime ?? default,
                SeatNumbers = b.BookingSeats?.Select(bs => bs.Seat?.SeatNumber ?? "").Where(s => !string.IsNullOrEmpty(s)).ToList() ?? new List<string>(),
                Status = b.Status.ToString(),
                TotalPrice = b.TotalPrice
            }).ToList();
        }
    }
}
