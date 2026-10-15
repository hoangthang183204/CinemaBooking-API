using System.Linq;
using CinemaBooking.Application.Interfaces.Repositories;
using CinemaBooking.Domain.Entities;
using CinemaBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Infrastructure.Repositories
{
    public class ShowTimeRepository : IShowTimeRepository
    {
        private readonly CinemaDbContext _context;

        public ShowTimeRepository(CinemaDbContext context)
        {
            _context = context;
        }

        public ShowTime? GetById(int id)
        {
            return _context.ShowTimes
                .Include(x => x.Room)
                .Include(x => x.Movie)
                .FirstOrDefault(x => x.Id == id);
        }

        public List<ShowTime> GetAll()
        {
            return _context.ShowTimes
                .Include(x => x.Room)
                .Include(x => x.Movie)
                .OrderBy(x => x.StartTime)
                .ToList();
        }

        public void Add(ShowTime showTime)
        {
            _context.ShowTimes.Add(showTime);
            _context.SaveChanges();
        }
    }
}
