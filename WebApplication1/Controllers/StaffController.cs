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
    [Route("api/staff")]
    [Authorize(Roles = "Staff,Admin")]
    public class StaffController : ControllerBase
    {
        private readonly IAdminService _admin;
        private readonly ILogger<StaffController> _logger;

        public StaffController(IAdminService admin, ILogger<StaffController> logger)
        {
            _admin = admin;
            _logger = logger;
        }

        // ========== DASHBOARD ==========
        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            var staffId = User.GetUserId();
            if (!staffId.HasValue)
                return Unauthorized(ApiResponse<string>.Fail("Không xác định được nhân viên"));
            return Ok(ApiResponse<StaffHomeDto>.Ok(_admin.GetStaffHome(staffId.Value)));
        }

        [HttpPost("shift/close")]
        public IActionResult CloseShift()
        {
            var staffId = User.GetUserId();
            if (!staffId.HasValue)
                return Unauthorized(ApiResponse<string>.Fail("Không xác định được nhân viên"));
            var report = _admin.CloseStaffShift(staffId.Value);
            return report == null
                ? BadRequest(ApiResponse<string>.Fail("Không có ca đang mở."))
                : Ok(ApiResponse<ShiftReportDto>.Ok(report, "Đã kết thúc ca"));
        }

        [HttpGet("reports")]
        public IActionResult Reports()
        {
            var staffId = User.GetUserId();
            if (!staffId.HasValue)
                return Unauthorized(ApiResponse<string>.Fail("Không xác định được nhân viên"));
            return Ok(ApiResponse<List<ShiftReportDto>>.Ok(_admin.ListStaffReports(staffId.Value)));
        }

        [HttpGet("reports/{id:int}")]
        public IActionResult Report(int id)
        {
            var staffId = User.GetUserId();
            if (!staffId.HasValue)
                return Unauthorized(ApiResponse<string>.Fail("Không xác định được nhân viên"));
            var report = _admin.GetStaffReport(staffId.Value, id);
            return report == null
                ? NotFound(ApiResponse<string>.Fail("Không thấy biên bản"))
                : Ok(ApiResponse<ShiftReportDto>.Ok(report));
        }

        [HttpPost("handover/accept")]
        public IActionResult AcceptHandover()
        {
            var staffId = User.GetUserId();
            if (!staffId.HasValue)
                return Unauthorized(ApiResponse<string>.Fail("Không xác định được nhân viên"));
            try
            {
                _admin.AcceptHandover(staffId.Value);
                return Ok(ApiResponse<string>.Ok("Đã nhận ca"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message));
            }
        }

        [HttpPost("shift/open")]
        public IActionResult OpenShift()
        {
            var staffId = User.GetUserId();
            if (!staffId.HasValue)
                return Unauthorized(ApiResponse<string>.Fail("Không xác định được nhân viên"));
            try
            {
                return Ok(ApiResponse<ShiftReportDto>.Ok(_admin.OpenStaffShift(staffId.Value), "Đã mở ca"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message));
            }
        }

        [HttpGet("lookup")]
        public IActionResult Lookup([FromQuery] string? q)
        {
            return Ok(ApiResponse<List<AdminBookingDto>>.Ok(_admin.LookupBookings(q)));
        }

        // ========== XEM BOOKINGS ==========
        [HttpGet("bookings")]
        public IActionResult GetBookings()
        {
            return Ok(ApiResponse<List<AdminBookingDto>>.Ok(_admin.GetAllBookings()));
        }

        [HttpGet("bookings/{id:int}")]
        public IActionResult GetBooking(int id)
        {
            var b = _admin.GetBookingDetail(id);
            return b == null
                ? NotFound(ApiResponse<string>.Fail("Không tìm thấy đơn"))
                : Ok(ApiResponse<StaffBookingDetailDto>.Ok(b));
        }

        // ========== TẠO BOOKING TẠI QUẦY ==========
        [HttpPost("bookings/create-at-counter")]
        public IActionResult CreateCounterBooking([FromBody] CreateCounterBookingRequest request)
        {
            var staffId = User.GetUserId();
            if (!staffId.HasValue)
                return Unauthorized(ApiResponse<string>.Fail("Không xác định được nhân viên"));

            if (request.SeatIds == null || request.SeatIds.Count == 0)
                return BadRequest(ApiResponse<string>.Fail("Chọn ít nhất 1 ghế"));

            if (request.SeatIds.Count > 10)
                return BadRequest(ApiResponse<string>.Fail("Tối đa 10 ghế mỗi đơn"));

            try
            {
                var result = _admin.CreateCounterBooking(request, staffId.Value);
                if (result == null)
                    return BadRequest(ApiResponse<string>.Fail(
                        "Không thể tạo đơn. Kiểm tra suất chiếu và ghế có thể đã bị đặt."));

                _logger.LogInformation("Staff {StaffId} created counter booking #{BookingId}",
                    staffId, result.BookingId);

                return Ok(ApiResponse<CounterBookingResponse>.Ok(result,
                    $"Tạo đơn #{result.BookingId} thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create counter booking failed");
                return BadRequest(ApiResponse<string>.Fail("Lỗi tạo đơn: " + ex.Message));
            }
        }

        // ========== XÁC NHẬN THANH TOÁN ==========
        [HttpPost("bookings/{id:int}/confirm-payment")]
        public IActionResult ConfirmPayment(int id, [FromBody] ConfirmPaymentRequest? request)
        {
            var staffId = User.GetUserId();
            if (!staffId.HasValue)
                return Unauthorized(ApiResponse<string>.Fail("Không xác định được nhân viên"));

            ConfirmPaymentResponse? result;
            try
            {
                result = _admin.ConfirmBookingPayment(
                    id, staffId.Value, request?.Note, request?.Method, request?.CashReceived);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message));
            }
            if (result == null)
                return BadRequest(ApiResponse<string>.Fail(
                    "Không thể xác nhận. Đơn không tồn tại hoặc đã thanh toán."));

            _logger.LogInformation("Staff {StaffId} confirmed payment for booking #{BookingId}",
                staffId, id);

            return Ok(ApiResponse<ConfirmPaymentResponse>.Ok(result,
                "Xác nhận thanh toán thành công"));
        }

        // ========== CHECK-IN QR ==========
        [HttpPost("checkin-qr")]
        public IActionResult CheckInByQR([FromBody] CheckInQRRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.QRCode))
                return BadRequest(ApiResponse<string>.Fail("QR Code không được rỗng"));

            var result = _admin.CheckInByQRCode(request.QRCode);
            if (result == null)
                return NotFound(ApiResponse<string>.Fail("Không tìm thấy vé với QR này"));

            _logger.LogInformation("Check-in booking #{BookingId}, seat {Seat}, tone {Tone}",
                result.BookingId, result.SeatNumber, result.Tone);

            return Ok(ApiResponse<CheckInResponse>.Ok(result, result.Message));
        }

        [HttpPost("bookings/{id:int}/exchange")]
        public IActionResult Exchange(int id, [FromBody] ExchangeBookingRequest request)
        {
            var staffId = User.GetUserId();
            if (!staffId.HasValue)
                return Unauthorized(ApiResponse<string>.Fail("Không xác định được nhân viên"));
            try
            {
                var result = _admin.ExchangeBooking(id, request, staffId.Value);
                return Ok(ApiResponse<ExchangeBookingResult>.Ok(result, result.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message));
            }
        }

        // ========== MARK PAID (nhanh) ==========
        [HttpPost("bookings/{id:int}/mark-paid")]
        public IActionResult MarkPaid(int id)
        {
            var ok = _admin.MarkBookingAsPaid(id, User.GetUserId() ?? 0);
            return ok
                ? Ok(ApiResponse<string>.Ok("Đã xác nhận thanh toán"))
                : NotFound(ApiResponse<string>.Fail("Không tìm thấy booking"));
        }

        [HttpPost("bookings/{id:int}/refund")]
        public IActionResult Refund(int id)
        {
            var ok = _admin.CompleteRefund(id);
            return ok
                ? Ok(ApiResponse<string>.Ok("Đã hoàn tiền tại quầy"))
                : BadRequest(ApiResponse<string>.Fail("Đơn này không ở trạng thái chờ hoàn tiền"));
        }

        // ========== CANCEL ==========
        [HttpPost("bookings/{id:int}/cancel")]
        public IActionResult Cancel(int id)
        {
            var ok = _admin.CancelBooking(id);
            return ok
                ? Ok(ApiResponse<string>.Ok("Đã hủy booking"))
                : NotFound(ApiResponse<string>.Fail("Không tìm thấy booking"));
        }
    }
}