using CinemaBooking.API.Extensions;
using CinemaBooking.Application.Common;
using CinemaBooking.Application.DTO;
using CinemaBooking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaBooking.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly ICreateBookingService _createBookingService;
        private readonly IBookingQueryService _bookingQuery;
        private readonly IAdminService _admin;

        public BookingsController(
            ICreateBookingService createBookingService,
            IBookingQueryService bookingQuery,
            IAdminService admin)
        {
            _createBookingService = createBookingService;
            _bookingQuery = bookingQuery;
            _admin = admin;
        }

        [HttpPost]
        [Authorize]
        public IActionResult Create([FromBody] CreateBookingRequest request)
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<string>.Fail("Token không hợp lệ"));

            request.UserId = userId.Value;

            var result = _createBookingService.Execute(request);
            if (!result.Success)
                return BadRequest(ApiResponse<object>.Fail(result.ErrorMessage ?? "Đặt vé thất bại"));

            return Ok(ApiResponse<CreateBookingResponse>.Ok(result.Response!, result.Response!.Message));
        }

        [HttpGet]
        public IActionResult GetBookings([FromQuery] int? userId)
        {
            var list = _bookingQuery.GetBookings(userId);
            return Ok(ApiResponse<List<BookingSummaryDto>>.Ok(list));
        }

        [HttpPost("{id:int}/cancel")]
        [Authorize]
        public IActionResult Cancel(int id)
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<string>.Fail("Token không hợp lệ"));

            var (ok, message) = _admin.CancelForCustomer(id, userId.Value);
            return ok
                ? Ok(ApiResponse<string>.Ok(message))
                : BadRequest(ApiResponse<string>.Fail(message));
        }

        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            var booking = _bookingQuery.GetById(id);
            if (booking == null)
                return NotFound(ApiResponse<string>.Fail("Không tìm thấy booking"));
            return Ok(ApiResponse<BookingDetailDto>.Ok(booking));
        }
    }
}