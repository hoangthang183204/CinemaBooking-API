using CinemaBooking.Application.Interfaces.Repositories;
using CinemaBooking.Domain.Entities;
using CinemaBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Infrastructure.Repositories
{
    public class SeatRepository : ISeatRepository
    {
        private readonly CinemaDbContext _context;

        public SeatRepository(CinemaDbContext context)
        {
            _context = context;
        }

        public Seat? GetById(int seatId)
        {
            return _context.Seats.FirstOrDefault(s => s.Id == seatId);
        }

        public List<Seat> GetSeatsByRoomId(int roomId)
        {
            return GetByRoomId(roomId);
        }

        public List<Seat> GetByRoomId(int roomId)
        {
            return _context.Seats
                .Where(s => s.RoomId == roomId)
                .OrderBy(s => s.SeatNumber)
                .ToList();
        }
    }
}
