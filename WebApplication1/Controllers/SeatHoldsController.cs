using CinemaBooking.API.Extensions;
using CinemaBooking.Application.Common;
using CinemaBooking.Application.DTO;
using CinemaBooking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaBooking.API.Controllers
{
    [ApiController]
    [Authorize]
    public class SeatHoldsController : ControllerBase
    {
        private readonly ISeatHoldService _seatHolds;

        public SeatHoldsController(ISeatHoldService seatHolds)
        {
            _seatHolds = seatHolds;
        }

        [HttpPost("api/showtimes/{showTimeId:int}/seat-holds")]
        public IActionResult Hold(int showTimeId, [FromBody] CreateSeatHoldRequest request)
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<string>.Fail("Token không hợp lệ"));

            var result = _seatHolds.Hold(showTimeId, userId.Value, request?.SeatIds ?? new List<int>());
            if (!result.Success)
                return BadRequest(ApiResponse<object>.Fail(result.ErrorMessage ?? "Không giữ được ghế"));

            return Ok(ApiResponse<SeatHoldCreatedDto>.Ok(result.Response!, "Đã giữ ghế trong 10 phút."));
        }

        [HttpDelete("api/seat-holds/{holdToken:guid}")]
        public IActionResult Release(Guid holdToken)
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<string>.Fail("Token không hợp lệ"));

            var released = _seatHolds.Release(holdToken, userId.Value);
            if (!released)
                return NotFound(ApiResponse<string>.Fail("Không tìm thấy lượt giữ ghế"));

            return Ok(ApiResponse<string>.Ok("Đã nhả ghế"));
        }

    }
}
