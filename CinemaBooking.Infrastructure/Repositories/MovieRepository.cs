using CinemaBooking.Application.Interfaces.Repositories;
using CinemaBooking.Domain.Entities;
using CinemaBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Infrastructure.Repositories
{
    public class MovieRepository : IMovieRepository
    {
        private readonly CinemaDbContext _context;

        public MovieRepository(CinemaDbContext context)
        {
            _context = context;
        }

        public Movie? GetById(int id)
        {
            return _context.Movies
                .FirstOrDefault(m => m.Id == id);
        }

        public List<Movie> GetAll()
        {
            return _context.Movies.ToList();
        }
    }
}
