using CinemaBooking.Domain.Entities;
using System.Collections.Generic;

namespace CinemaBooking.Application.Interfaces.Repositories
{
    public interface IMovieRepository
    {
        List<Movie> GetAll();
        Movie? GetById(int id);
    }
}
