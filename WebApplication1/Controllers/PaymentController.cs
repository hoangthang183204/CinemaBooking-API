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
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IAdminService _admin;
        private readonly IConfiguration _config;

        public PaymentController(IPaymentService paymentService, IAdminService admin, IConfiguration config)
        {
            _paymentService = paymentService;
            _admin = admin;
            _config = config;
        }

        [HttpGet("methods")]
        public IActionResult Methods()
        {
            var vnpay = !string.IsNullOrWhiteSpace(_config["Payment:VNPay:TmnCode"])
                && !string.IsNullOrWhiteSpace(_config["Payment:VNPay:HashSecret"]);
            return Ok(ApiResponse<object>.Ok(new
            {
                vnpay,
                bankBin = _config["VietQR:BankBin"] ?? "",
                accountNumber = _config["VietQR:AccountNumber"] ?? "",
                accountName = _config["VietQR:AccountName"] ?? ""
            }));
        }

        [HttpPost("bank-ipn")]
        [AllowAnonymous]
        public IActionResult BankIpn([FromBody] System.Text.Json.JsonElement body)
        {
            var token = _config["Payment:BankNotify:Token"] ?? "";
            var auth = Request.Headers["Authorization"].ToString();
            var headerToken = Request.Headers["X-Notify-Token"].ToString();
            if (string.IsNullOrWhiteSpace(token) || (auth != $"Apikey {token}" && headerToken != token))
                return Unauthorized(ApiResponse<string>.Fail("Sai mã thông báo ngân hàng"));

            var content = FirstString(body, "content", "description", "addInfo");
            var amount = FirstDecimal(body, "transferAmount", "amount");
            var transferType = FirstString(body, "transferType");
            if (!string.IsNullOrWhiteSpace(transferType) && !transferType.Equals("in", StringComparison.OrdinalIgnoreCase))
                return Ok(ApiResponse<string>.Ok("Bỏ qua giao dịch tiền ra"));

            var ok = _admin.TryConfirmBankTransfer(content, amount);
            return ok
                ? Ok(ApiResponse<string>.Ok("Đã xác nhận thanh toán"))
                : Ok(ApiResponse<string>.Fail("Không khớp đơn hoặc số tiền"));
        }

        private static string FirstString(System.Text.Json.JsonElement body, params string[] names)
        {
            foreach (var name in names)
            {
                if (body.ValueKind == System.Text.Json.JsonValueKind.Object
                    && body.TryGetProperty(name, out var value)
                    && value.ValueKind == System.Text.Json.JsonValueKind.String)
                    return value.GetString() ?? "";
            }
            return "";
        }

        private static decimal FirstDecimal(System.Text.Json.JsonElement body, params string[] names)
        {
            foreach (var name in names)
            {
                if (body.ValueKind != System.Text.Json.JsonValueKind.Object || !body.TryGetProperty(name, out var value))
                    continue;
                if (value.ValueKind == System.Text.Json.JsonValueKind.Number && value.TryGetDecimal(out var number))
                    return number;
                if (value.ValueKind == System.Text.Json.JsonValueKind.String && decimal.TryParse(value.GetString(), out var parsed))
                    return parsed;
            }
            return 0;
        }

        [HttpPost("bookings/{bookingId:int}/payment-url")]
        [Authorize]
        public IActionResult CreatePaymentUrl(int bookingId, [FromBody] CreatePaymentUrlRequest request)
        {
            if (!User.GetUserId().HasValue)
                return Unauthorized(ApiResponse<string>.Fail("Token không hợp lệ"));

            var frontendUrl = _config["Frontend:BaseUrl"] ?? "http://localhost:5173";
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";

            var result = _paymentService.CreatePaymentUrl(bookingId, request, frontendUrl, ipAddress);

            if (result == null)
            {
                var missingVnPay = string.Equals(request.Method, "vnpay", StringComparison.OrdinalIgnoreCase)
                    && (string.IsNullOrWhiteSpace(_config["Payment:VNPay:TmnCode"])
                        || string.IsNullOrWhiteSpace(_config["Payment:VNPay:HashSecret"]));
                return BadRequest(ApiResponse<string>.Fail(missingVnPay
                    ? "VNPay từ chối mã demo công khai (lỗi 99). Hãy đăng ký sandbox rồi điền TmnCode và HashSecret trong appsettings."
                    : "Không tạo được link thanh toán"));
            }

            return Ok(ApiResponse<CreatePaymentUrlResponse>.Ok(result));
        }

        [HttpGet("callback/vnpay")]
        public IActionResult CallbackVnPay()
        {
            var query = Request.Query.ToDictionary(q => q.Key, q => q.Value.ToString() ?? "");
            _paymentService.HandleVnPayCallback(query, out var bookingId, out var message);
            return Redirect(FrontendBookingSuccess(bookingId, message));
        }

        [HttpGet("ipn/vnpay")]
        public IActionResult IpnVnPay()
        {
            var query = Request.Query.ToDictionary(q => q.Key, q => q.Value.ToString() ?? "");
            var ok = _paymentService.HandleVnPayCallback(query, out _, out var message);
            if (ok) return Ok(new { RspCode = "00", Message = "Confirm Success" });
            var code = message.Contains("khong tim thay", StringComparison.OrdinalIgnoreCase) ? "01"
                : message.Contains("chu ky", StringComparison.OrdinalIgnoreCase) ? "97"
                : "99";
            return Ok(new { RspCode = code, Message = message });
        }

        [HttpGet("callback/momo")]
        public IActionResult CallbackMomo()
        {
            var query = Request.Query.ToDictionary(q => q.Key, q => q.Value.ToString() ?? "");
            _paymentService.HandleMomoCallback(query, out var bookingId, out var message);
            return Redirect(FrontendBookingSuccess(bookingId, message));
        }

        private string FrontendBookingSuccess(int? bookingId, string message)
        {
            var redirectBase = _config["Frontend:BaseUrl"] ?? "http://localhost:5173";
            return $"{redirectBase.TrimEnd('/')}/booking-success?bookingId={bookingId}&status={Uri.EscapeDataString(message)}";
        }
    }
}
