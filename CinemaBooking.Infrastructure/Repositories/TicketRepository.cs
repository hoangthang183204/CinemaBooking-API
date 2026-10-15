using CinemaBooking.Application.Interfaces.Repositories;
using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Enums;
using CinemaBooking.Infrastructure.Data;

namespace CinemaBooking.Infrastructure.Repositories
{
    public class TicketRepository : ITicketRepository
    {
        private readonly CinemaDbContext _context;

        public TicketRepository(CinemaDbContext context)
        {
            _context = context;
        }

        public List<int> GetBookedSeatIds(int showTimeId)
        {
            return _context.Tickets
                .Where(t => t.ShowTimeId == showTimeId)
                .Select(t => t.SeatId)
                .ToList();
        }

        public bool IsSeatBooked(int showTimeId, int seatId)
        {
            return _context.Tickets
                .Any(t => t.ShowTimeId == showTimeId && t.SeatId == seatId);
        }

        public void Add(Ticket ticket)
        {
            _context.Tickets.Add(ticket);
            _context.SaveChanges();
        }

        public List<int> GetBookedSeatIds(int showTimeId, TicketStatus status)
        {
            return _context.Tickets
                .Where(t => t.ShowTimeId == showTimeId)
                .Select(t => t.SeatId)
                .ToList();
        }
    }
}
