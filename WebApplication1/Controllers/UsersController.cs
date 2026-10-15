using CinemaBooking.API.Extensions;
using CinemaBooking.Application.Common;
using CinemaBooking.Application.DTO;
using CinemaBooking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaBooking.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]                                     // ← bắt buộc login
    public class UsersController : ControllerBase
    {
        private readonly IBookingQueryService _bookingQuery;

        public UsersController(IBookingQueryService bookingQuery)
        {
            _bookingQuery = bookingQuery;
        }

        [HttpGet("my-bookings")]
        public IActionResult GetMyBookings()
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<string>.Fail("Token không hợp lệ"));

            var list = _bookingQuery.GetBookings(userId.Value);
            return Ok(ApiResponse<List<BookingSummaryDto>>.Ok(list));
        }
    }
}