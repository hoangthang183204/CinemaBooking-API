using CinemaBooking.API.Extensions;
using CinemaBooking.Application.Common;
using CinemaBooking.Application.DTO.Admin;
using CinemaBooking.Application.DTO.Staff;
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

            try
            {
                var st = _admin.CreateShowTime(req);
                return st == null
                    ? BadRequest(ApiResponse<string>.Fail("Phim hoặc phòng không tồn tại"))
                    : Ok(ApiResponse<AdminShowTimeDto>.Ok(st, "Thêm suất chiếu thành công"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message));
            }
        }

        [HttpPut("showtimes/{id}")]
        public IActionResult UpdateShowTime(int id, [FromBody] CreateUpdateShowTimeRequest req)
        {
            if (ShowTimeClock.HasStarted(req.StartTime))
                return BadRequest(ApiResponse<string>.Fail("Giờ chiếu phải ở tương lai."));

            try
            {
                var st = _admin.UpdateShowTime(id, req);
                return st == null
                    ? NotFound(ApiResponse<string>.Fail("Không tìm thấy suất chiếu"))
                    : Ok(ApiResponse<AdminShowTimeDto>.Ok(st, "Cập nhật thành công"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message));
            }
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

        [HttpGet("staff")]
        public IActionResult StaffDirectory()
        {
            return Ok(ApiResponse<List<StaffProfileDto>>.Ok(_admin.GetStaffDirectory()));
        }

        [HttpGet("staff/{id:int}")]
        public IActionResult StaffProfile(int id)
        {
            var staff = _admin.GetStaffProfile(id);
            return staff == null
                ? NotFound(ApiResponse<string>.Fail("Không tìm thấy nhân viên"))
                : Ok(ApiResponse<StaffProfileDto>.Ok(staff));
        }

        [HttpPost("staff")]
        public IActionResult CreateStaff([FromBody] SaveStaffRequest req)
        {
            try
            {
                var staff = _admin.CreateStaff(req);
                return Ok(ApiResponse<StaffProfileDto>.Ok(staff, "Đã tạo nhân viên"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message));
            }
        }

        [HttpPut("staff/{id:int}")]
        public IActionResult UpdateStaff(int id, [FromBody] SaveStaffRequest req)
        {
            try
            {
                var staff = _admin.UpdateStaff(id, req);
                return staff == null
                    ? NotFound(ApiResponse<string>.Fail("Không tìm thấy nhân viên"))
                    : Ok(ApiResponse<StaffProfileDto>.Ok(staff, "Đã cập nhật nhân viên"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message));
            }
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

        [HttpGet("rooms/{id}/seats")]
        public IActionResult GetRoomSeats(int id)
        {
            var layout = _admin.GetRoomLayout(id);
            return layout == null
                ? NotFound(ApiResponse<string>.Fail("Không tìm thấy phòng"))
                : Ok(ApiResponse<RoomLayoutDto>.Ok(layout));
        }

        [HttpPut("rooms/{id}/seats")]
        public IActionResult SaveRoomSeats(int id, [FromBody] UpdateSeatLayoutRequest req)
        {
            var layout = _admin.SaveRoomLayout(id, req);
            return layout == null
                ? NotFound(ApiResponse<string>.Fail("Không tìm thấy phòng"))
                : Ok(ApiResponse<RoomLayoutDto>.Ok(layout, "Đã lưu sơ đồ ghế"));
        }

        [HttpGet("promotions")]
        public IActionResult GetPromotions()
        {
            return Ok(ApiResponse<List<PromotionDto>>.Ok(_admin.GetPromotions()));
        }

        [HttpPost("promotions")]
        public IActionResult CreatePromotion([FromBody] SavePromotionRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                return BadRequest(ApiResponse<string>.Fail("Nhập tên chương trình"));
            return Ok(ApiResponse<PromotionDto>.Ok(_admin.CreatePromotion(req), "Đã thêm khuyến mãi"));
        }

        [HttpPut("promotions/{id}")]
        public IActionResult UpdatePromotion(int id, [FromBody] SavePromotionRequest req)
        {
            var p = _admin.UpdatePromotion(id, req);
            return p == null
                ? NotFound(ApiResponse<string>.Fail("Không tìm thấy khuyến mãi"))
                : Ok(ApiResponse<PromotionDto>.Ok(p, "Đã cập nhật"));
        }

        [HttpDelete("promotions/{id}")]
        public IActionResult DeletePromotion(int id)
        {
            return _admin.DeletePromotion(id)
                ? Ok(ApiResponse<string>.Ok("Đã xóa khuyến mãi"))
                : NotFound(ApiResponse<string>.Fail("Không tìm thấy khuyến mãi"));
        }

        [HttpGet("reports")]
        public IActionResult GetReport([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string groupBy = "movie")
        {
            var end = to ?? DateTime.Today;
            var start = from ?? end.AddDays(-13);
            return Ok(ApiResponse<ReportDto>.Ok(_admin.GetReport(start, end, groupBy)));
        }

        [HttpGet("reports/excel")]
        public IActionResult ExportReport([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string groupBy = "movie")
        {
            var end = to ?? DateTime.Today;
            var start = from ?? end.AddDays(-13);
            var bytes = _admin.ExportReportExcel(start, end, groupBy);
            var name = $"bao-cao-{start:yyyyMMdd}-{end:yyyyMMdd}.xls";
            return File(bytes, "application/vnd.ms-excel", name);
        }

        [HttpGet("settings")]
        public IActionResult GetSettings()
        {
            return Ok(ApiResponse<SystemSettingsDto>.Ok(_admin.GetSettings()));
        }

        [HttpPut("settings")]
        public IActionResult SaveSettings([FromBody] SystemSettingsDto req)
        {
            return Ok(ApiResponse<SystemSettingsDto>.Ok(_admin.SaveSettings(req), "Đã lưu cấu hình"));
        }

        [HttpGet("shifts")]
        public IActionResult GetShifts()
        {
            return Ok(ApiResponse<List<AdminShiftDto>>.Ok(_admin.GetShifts()));
        }

        [HttpGet("shifts/{id:int}")]
        public IActionResult GetShift(int id)
        {
            var shift = _admin.GetShift(id);
            return shift == null
                ? NotFound(ApiResponse<string>.Fail("Không tìm thấy ca"))
                : Ok(ApiResponse<AdminShiftDto>.Ok(shift));
        }

        [HttpGet("staff-options")]
        public IActionResult StaffOptions()
        {
            return Ok(ApiResponse<List<StaffOptionDto>>.Ok(_admin.GetStaffOptions()));
        }

        [HttpPost("shifts/assign")]
        public IActionResult AssignShift([FromBody] AssignShiftRequest request)
        {
            var adminId = User.GetUserId();
            if (!adminId.HasValue)
                return Unauthorized(ApiResponse<string>.Fail("Không xác định được quản trị"));
            try
            {
                var shift = _admin.AssignShift(request.StaffId, adminId.Value, request.Note);
                return Ok(ApiResponse<AdminShiftDto>.Ok(shift, "Đã gán ca"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message));
            }
        }

        [HttpPost("shifts/{id:int}/approve")]
        public IActionResult ApproveShift(int id, [FromBody] ApproveShiftRequest request)
        {
            var adminId = User.GetUserId();
            if (!adminId.HasValue)
                return Unauthorized(ApiResponse<string>.Fail("Không xác định được quản trị"));
            try
            {
                var shift = _admin.ApproveShift(id, adminId.Value, request.CountedCash, request.Note);
                return Ok(ApiResponse<AdminShiftDto>.Ok(shift, "Đã duyệt ca"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message));
            }
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