using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CinemaBooking.Application.Interfaces.Repositories;
using CinemaBooking.Domain.Entities;

namespace CinemaBooking.Application.Services
{
    public class MovieService
    {
        private readonly IMovieRepository _movieRepository;
        public MovieService(IMovieRepository movieRepository)
        {
            _movieRepository = movieRepository;
        }

        public List<Movie> GetMovies()
        {
            return _movieRepository.GetAll();
        }

        public Movie? GetById(int id)
        {
            return _movieRepository.GetById(id);
        }
    }
}
