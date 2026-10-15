using CinemaBooking.Application.Interfaces.Repositories;
using CinemaBooking.Domain.Entities;
using CinemaBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Infrastructure.Repositories
{
    public class RoomRepository : IRoomRepository
    {
        private readonly CinemaDbContext _context;

        public RoomRepository(CinemaDbContext context)
        {
            _context = context;
        }

        public List<Room> GetAll()
        {
            return _context.Rooms.OrderBy(r => r.Name).ToList();
        }

        public Room? GetById(int id)
        {
            return _context.Rooms.FirstOrDefault(r => r.Id == id);
        }
    }
}
