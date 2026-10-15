using CinemaBooking.Application.Common;
using CinemaBooking.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CinemaBooking.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MoviesController : ControllerBase
    {
        private readonly MovieService _movieService;
        public MoviesController(MovieService movieService) => _movieService = movieService;

        [HttpGet]
        public IActionResult GetAll()
        {
            var movies = _movieService.GetMovies();
            return Ok(ApiResponse<List<Domain.Entities.Movie>>.Ok(movies));
        }

        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            var movie = _movieService.GetById(id);
            if (movie == null)
                return NotFound(ApiResponse<string>.Fail("Không tìm thấy phim"));
            return Ok(ApiResponse<Domain.Entities.Movie>.Ok(movie));
        }
    }
}