using CinemaBooking.Application.Common;
using CinemaBooking.Application.Services;
using CinemaBooking.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace CinemaBooking.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomsController : ControllerBase
    {
        private readonly RoomService _roomService;
        public RoomsController(RoomService roomService) => _roomService = roomService;

        [HttpGet]
        public IActionResult GetAll() =>
            Ok(ApiResponse<List<Room>>.Ok(_roomService.GetRooms()));

        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            var room = _roomService.GetById(id);
            return room == null
                ? NotFound(ApiResponse<string>.Fail("Không tìm thấy phòng"))
                : Ok(ApiResponse<Room>.Ok(room));
        }
    }
}