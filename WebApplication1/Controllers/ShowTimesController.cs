using CinemaBooking.API.Extensions;
using CinemaBooking.API.Models;
using CinemaBooking.Application.Common;
using CinemaBooking.Application.DTO;
using CinemaBooking.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CinemaBooking.API.Controllers
{
    [ApiController]
    [Route("api/showtimes")]
    public class ShowTimesController : ControllerBase
    {
        private readonly ShowTimeService _service;
        public ShowTimesController(ShowTimeService service) => _service = service;

        [HttpGet]
        public IActionResult GetAll() =>
            Ok(ApiResponse<List<ShowTimeDto>>.Ok(_service.GetShowTimes()));

        [HttpGet("{id:int}/room-seats")]
        public IActionResult GetRoomSeats(int id)
        {
            try
            {
                var result = _service.GetRoomSeatsByShowTime(id, User.GetUserId());
                return result == null
                    ? NotFound(ApiResponse<string>.Fail("Không tìm thấy suất chiếu"))
                    : Ok(ApiResponse<RoomSeatDto>.Ok(result));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message));
            }
        }

        [HttpPost]
        public IActionResult Create([FromBody] CreateShowTimeRequest request)
        {
            if (request == null)
                return BadRequest(ApiResponse<string>.Fail("Thiếu dữ liệu"));
            if (ShowTimeClock.HasStarted(request.StartTime))
                return BadRequest(ApiResponse<string>.Fail("Giờ chiếu phải ở tương lai."));

            var created = _service.CreateShowTime(request.MovieId, request.RoomId, request.StartTime);
            return created == null
                ? BadRequest(ApiResponse<string>.Fail("Phim hoặc phòng không tồn tại"))
                : Ok(ApiResponse<ShowTimeDto>.Ok(created));
        }
    }
}