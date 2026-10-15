using CinemaBooking.Application.Common;
using CinemaBooking.Application.DTO.Admin;
using CinemaBooking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaBooking.API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _admin;

        public AdminController(IAdminService admin) => _admin = admin;

        // ========== DASHBOARD ==========
        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            return Ok(ApiResponse<DashboardStatsDto>.Ok(_admin.GetDashboardStats()));
        }

        // ========== MOVIES ==========
        [HttpGet("movies")]
        public IActionResult GetMovies()
        {
            return Ok(ApiResponse<List<AdminMovieDto>>.Ok(_admin.GetAllMovies()));
        }

        [HttpGet("movies/{id}")]
        public IActionResult GetMovie(int id)
        {
            var m = _admin.GetMovieById(id);
            return m == null
                ? NotFound(ApiResponse<string>.Fail("Không tìm thấy phim"))
                : Ok(ApiResponse<AdminMovieDto>.Ok(m));
        }

        [HttpPost("movies")]
        public IActionResult CreateMovie([FromBody] CreateUpdateMovieRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Title))
                return BadRequest(ApiResponse<string>.Fail("Tên phim không được rỗng"));

            var m = _admin.CreateMovie(req);
            return Ok(ApiResponse<AdminMovieDto>.Ok(m, "Thêm phim thành công"));
        }

        [HttpPut("movies/{id}")]
        public IActionResult UpdateMovie(int id, [FromBody] CreateUpdateMovieRequest req)
        {
            var m = _admin.UpdateMovie(id, req);
            return m == null
                ? NotFound(ApiResponse<string>.Fail("Không tìm thấy phim"))
                : Ok(ApiResponse<AdminMovieDto>.Ok(m, "Cập nhật phim thành công"));
        }

        [HttpDelete("movies/{id}")]
        public IActionResult DeleteMovie(int id)
        {
            var ok = _admin.DeleteMovie(id);
            return ok
                ? Ok(ApiResponse<string>.Ok("Đã xử lý xóa phim"))
                : NotFound(ApiResponse<string>.Fail("Không tìm thấy phim"));
        }

        // ========== ROOMS ==========
        [HttpGet("rooms")]
        public IActionResult GetRooms()
        {
            return Ok(ApiResponse<List<AdminRoomDto>>.Ok(_admin.GetAllRooms()));
        }

        [HttpGet("rooms/{id}")]
        public IActionResult GetRoom(int id)
        {
            var r = _admin.GetRoomById(id);
            return r == null
                ? NotFound(ApiResponse<string>.Fail("Không tìm thấy phòng"))
                : Ok(ApiResponse<AdminRoomDto>.Ok(r));
        }

        [HttpPost("rooms")]
        public IActionResult CreateRoom([FromBody] CreateUpdateRoomRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                return BadRequest(ApiResponse<string>.Fail("Tên phòng không được rỗng"));

            var r = _admin.CreateRoom(req);
            return Ok(ApiResponse<AdminRoomDto>.Ok(r, "Thêm phòng thành công"));
        }

        [HttpPut("rooms/{id}")]
        public IActionResult UpdateRoom(int id, [FromBody] CreateUpdateRoomRequest req)
        {
            var r = _admin.UpdateRoom(id, req);
            return r == null
                ? NotFound(ApiResponse<string>.Fail("Không tìm thấy phòng"))
                : Ok(ApiResponse<AdminRoomDto>.Ok(r, "Cập nhật phòng thành công"));
        }

        [HttpDelete("rooms/{id}")]
        public IActionResult DeleteRoom(int id)
        {
            var ok = _admin.DeleteRoom(id);
            return ok
                ? Ok(ApiResponse<string>.Ok("Đã xóa phòng"))
                : BadRequest(ApiResponse<string>.Fail("Không thể xóa phòng này (có suất chiếu)"));
        }

        // ========== SHOWTIMES ==========
        [HttpGet("showtimes")]
        public IActionResult GetShowTimes()
        {
            return Ok(ApiResponse<List<AdminShowTimeDto>>.Ok(_admin.GetAllShowTimes()));
        }

        [HttpPost("showtimes")]
        public IActionResult CreateShowTime([FromBody] CreateUpdateShowTimeRequest req)
        {
            if (ShowTimeClock.HasStarted(req.StartTime))
                return BadRequest(ApiResponse<string>.Fail("Giờ chiếu phải ở tương lai."));

            var st = _admin.CreateShowTime(req);
            return st == null
                ? BadRequest(ApiResponse<string>.Fail("Phim hoặc phòng không tồn tại"))
                : Ok(ApiResponse<AdminShowTimeDto>.Ok(st, "Thêm suất chiếu thành công"));
        }

        [HttpPut("showtimes/{id}")]
        public IActionResult UpdateShowTime(int id, [FromBody] CreateUpdateShowTimeRequest req)
        {
            if (ShowTimeClock.HasStarted(req.StartTime))
                return BadRequest(ApiResponse<string>.Fail("Giờ chiếu phải ở tương lai."));

            var st = _admin.UpdateShowTime(id, req);
            return st == null
                ? NotFound(ApiResponse<string>.Fail("Không tìm thấy suất chiếu"))
                : Ok(ApiResponse<AdminShowTimeDto>.Ok(st, "Cập nhật thành công"));
        }

        [HttpDelete("showtimes/{id}")]
        public IActionResult DeleteShowTime(int id)
        {
            var ok = _admin.DeleteShowTime(id);
            return ok
                ? Ok(ApiResponse<string>.Ok("Đã xóa suất chiếu"))
                : BadRequest(ApiResponse<string>.Fail("Không thể xóa (đã có booking)"));
        }

        // ========== USERS ==========
        [HttpGet("users")]
        public IActionResult GetUsers()
        {
            return Ok(ApiResponse<List<AdminUserDto>>.Ok(_admin.GetAllUsers()));
        }

        [HttpPut("users/{id}/role")]
        public IActionResult UpdateUserRole(int id, [FromBody] UpdateUserRoleRequest req)
        {
            var u = _admin.UpdateUserRole(id, req.Role);
            return u == null
                ? NotFound(ApiResponse<string>.Fail("Không tìm thấy user"))
                : Ok(ApiResponse<AdminUserDto>.Ok(u, "Đã cập nhật role"));
        }

        [HttpPut("users/{id}/status")]
        public IActionResult UpdateUserStatus(int id, [FromBody] UpdateUserStatusRequest req)
        {
            var u = _admin.UpdateUserStatus(id, req.IsActive);
            return u == null
                ? NotFound(ApiResponse<string>.Fail("Không tìm thấy user"))
                : Ok(ApiResponse<AdminUserDto>.Ok(u, "Đã cập nhật trạng thái"));
        }

        // ========== BOOKINGS ==========
        [HttpGet("bookings")]
        public IActionResult GetBookings()
        {
            return Ok(ApiResponse<List<AdminBookingDto>>.Ok(_admin.GetAllBookings()));
        }

        [HttpPost("bookings/{id}/cancel")]
        public IActionResult CancelBooking(int id)
        {
            var ok = _admin.CancelBooking(id);
            return ok
                ? Ok(ApiResponse<string>.Ok("Đã hủy booking"))
                : NotFound(ApiResponse<string>.Fail("Không tìm thấy booking"));
        }

        [HttpPost("bookings/{id}/refund")]
        public IActionResult RefundBooking(int id)
        {
            var ok = _admin.CompleteRefund(id);
            return ok
                ? Ok(ApiResponse<string>.Ok("Đã hoàn tiền tại quầy"))
                : BadRequest(ApiResponse<string>.Fail("Đơn này không ở trạng thái chờ hoàn tiền"));
        }

        [HttpPost("bookings/{id}/mark-paid")]
        public IActionResult MarkPaid(int id)
        {
            var ok = _admin.MarkBookingAsPaid(id);
            return ok
                ? Ok(ApiResponse<string>.Ok("Đã đánh dấu đã thanh toán"))
                : NotFound(ApiResponse<string>.Fail("Không tìm thấy booking"));
        }
    }
}