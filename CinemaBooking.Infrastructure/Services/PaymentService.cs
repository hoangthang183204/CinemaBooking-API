using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System;
using CinemaBooking.Application.DTO;
using CinemaBooking.Application.Interfaces;
using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Enums;
using CinemaBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CinemaBooking.Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly CinemaDbContext _context;
        private readonly IConfiguration _config;

        public PaymentService(CinemaDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public CreatePaymentUrlResponse? CreatePaymentUrl(int bookingId, CreatePaymentUrlRequest request, string returnBaseUrl, string ipAddress)
        {
            var booking = _context.Bookings
                .Include(b => b.BookingSeats)
                .FirstOrDefault(b => b.Id == bookingId);
            if (booking == null || booking.Status == BookingStatus.Paid)
                return null;

            var method = request.Method?.ToLowerInvariant();
            if (string.IsNullOrEmpty(method))
                return null;

            var amount = (long)booking.TotalPrice;
            var orderId = $"{bookingId}{VietnamNow():yyyyMMddHHmmss}";

            var payment = new Payment
            {
                BookingId = bookingId,
                Amount = booking.TotalPrice,
                Method = method == "vnpay" ? PaymentMethod.VNPay : method == "momo" ? PaymentMethod.Momo : PaymentMethod.Stripe,
                Status = PaymentStatus.Pending,
                GatewayOrderId = orderId,
                CreatedAt = DateTime.UtcNow
            };
            _context.Payments.Add(payment);
            _context.SaveChanges();

            string paymentUrl = "";
            if (method == "vnpay")
                paymentUrl = BuildVnPayUrl(payment.Id, amount, orderId, returnBaseUrl, ipAddress);
            else if (method == "momo")
                paymentUrl = BuildMomoUrl(payment.Id, amount, orderId, returnBaseUrl);
            else if (method == "stripe")
                paymentUrl = BuildStripeUrl(payment.Id, amount, orderId, returnBaseUrl);

            if (string.IsNullOrEmpty(paymentUrl))
                return null;

            return new CreatePaymentUrlResponse
            {
                PaymentId = payment.Id,
                PaymentUrl = paymentUrl
            };
        }

        private string BuildVnPayUrl(int paymentId, long amountVnd, string orderId, string returnBaseUrl, string ipAddress)
        {
            var hashSecret = _config["Payment:VNPay:HashSecret"] ?? "";
            var tmnCode = _config["Payment:VNPay:TmnCode"] ?? "";
            var vnpUrl = _config["Payment:VNPay:Url"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
            if (string.IsNullOrWhiteSpace(hashSecret) || string.IsNullOrWhiteSpace(tmnCode))
                return "";
            if (!Uri.TryCreate(vnpUrl, UriKind.Absolute, out var gateway)
                || gateway.Scheme != Uri.UriSchemeHttps
                || !gateway.Host.EndsWith("vnpayment.vn", StringComparison.OrdinalIgnoreCase))
                return "";

            // ReturnUrl phải trỏ về API (không phải frontend) — giống MoMo dùng Api:PublicUrl.
            var apiBase = (_config["Api:PublicUrl"] ?? returnBaseUrl ?? "http://localhost:5065").TrimEnd('/');
            var returnUrl = $"{apiBase}/api/payment/callback/vnpay";
            var now = VietnamNow();
            var ip = string.IsNullOrWhiteSpace(ipAddress) || ipAddress == "::1" ? "127.0.0.1" : ipAddress;

            var vnpParams = new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                { "vnp_Version", "2.1.0" },
                { "vnp_Command", "pay" },
                { "vnp_TmnCode", tmnCode },
                { "vnp_Amount", (amountVnd * 100).ToString() },
                { "vnp_CreateDate", now.ToString("yyyyMMddHHmmss") },
                { "vnp_CurrCode", "VND" },
                { "vnp_IpAddr", ip },
                { "vnp_Locale", "vn" },
                { "vnp_OrderInfo", $"Thanh toan ve {paymentId}" },
                { "vnp_OrderType", "other" },
                { "vnp_ReturnUrl", returnUrl },
                { "vnp_TxnRef", orderId }
            };

            var signData = SignData(vnpParams);
            var secureHash = HmacSha512(hashSecret, signData);
            return vnpUrl + "?" + signData + "&vnp_SecureHash=" + secureHash;
        }

        private string BuildMomoUrl(int paymentId, long amountVnd, string orderId, string returnBaseUrl)
        {
            var secretKey = _config["Payment:MoMo:SecretKey"] ?? "";
            var partnerCode = _config["Payment:MoMo:PartnerCode"] ?? "";
            var accessKey = _config["Payment:MoMo:AccessKey"] ?? "";
            if (string.IsNullOrWhiteSpace(secretKey) || string.IsNullOrWhiteSpace(partnerCode) || string.IsNullOrWhiteSpace(accessKey))
                throw new InvalidOperationException("Chưa cấu hình MoMo. Điền PartnerCode, AccessKey và SecretKey.");

            var endpoint = _config["Payment:MoMo:Endpoint"] ?? "https://test-payment.momo.vn/v2/gateway/api/create";
            if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var endpointUri)
                || endpointUri.Scheme != Uri.UriSchemeHttps
                || !endpointUri.Host.EndsWith("momo.vn", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Địa chỉ cổng MoMo không hợp lệ.");

            var apiBase = (_config["Api:PublicUrl"] ?? "http://localhost:5065").TrimEnd('/');
            var redirectUrl = $"{apiBase}/api/payment/callback/momo";
            var ipnUrl = $"{apiBase}/api/payment/ipn/momo";
            var requestId = orderId;
            var orderInfo = $"Thanh toan ve {paymentId}";
            var extraData = "";
            var requestType = "captureWallet";
            var rawSignature =
                $"accessKey={accessKey}&amount={amountVnd}&extraData={extraData}&ipnUrl={ipnUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={partnerCode}&redirectUrl={redirectUrl}&requestId={requestId}&requestType={requestType}";
            var signature = HmacSha256(secretKey, rawSignature);

            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
            var body = new
            {
                partnerCode,
                partnerName = "National Cinema",
                storeId = "CinemaBooking",
                requestId,
                amount = amountVnd,
                orderId,
                orderInfo,
                redirectUrl,
                ipnUrl,
                lang = "vi",
                extraData,
                requestType,
                signature,
                autoCapture = true
            };
            var json = System.Text.Json.JsonSerializer.Serialize(body);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = client.PostAsync(endpoint, content).GetAwaiter().GetResult();
            var respJson = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            using var doc = System.Text.Json.JsonDocument.Parse(string.IsNullOrWhiteSpace(respJson) ? "{}" : respJson);
            var root = doc.RootElement;
            var resultCode = root.TryGetProperty("resultCode", out var codeEl) && codeEl.TryGetInt32(out var code) ? code : -1;
            if (resultCode == 0 && root.TryGetProperty("payUrl", out var payUrlEl))
            {
                var payUrl = payUrlEl.GetString() ?? "";
                if (!string.IsNullOrWhiteSpace(payUrl)) return payUrl;
            }

            var momoMessage = root.TryGetProperty("message", out var messageEl) ? messageEl.GetString() : "MoMo không tạo được link thanh toán";
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(momoMessage) ? "MoMo không tạo được link thanh toán" : momoMessage);
        }

        private string BuildStripeUrl(int paymentId, long amountVnd, string orderId, string returnBaseUrl)
        {
            // Stripe tính bằng cent. Giả sử 1 VND = 0.00004 USD -> amountVnd VND ≈ amountVnd * 0.00004 USD = amountVnd * 4 / 100000 cent. Hoặc dùng Stripe với VND (nếu hỗ trợ).
            // Đơn giản: trả về URL checkout tĩnh hoặc cần Stripe SDK. Ở đây trả về returnBaseUrl/booking-success với query test để frontend có thể hiển thị "Stripe chưa cấu hình".
            var secretKey = _config["Payment:Stripe:SecretKey"];
            if (string.IsNullOrEmpty(secretKey))
                return $"{returnBaseUrl.TrimEnd('/')}/booking-success?bookingId=0&payment=stripe&status=config_required";
            // TODO: Stripe Checkout Session create. Tạm redirect về success với message.
            return $"{returnBaseUrl.TrimEnd('/')}/booking-success?bookingId={paymentId}&payment=stripe&status=pending";
        }

        public bool HandleVnPayCallback(IReadOnlyDictionary<string, string> query, out int bookingId, out string message)
        {
            bookingId = 0;
            message = "";
            var vnpParams = query.Where(kv => kv.Key.StartsWith("vnp_", StringComparison.OrdinalIgnoreCase))
                .OrderBy(kv => kv.Key, StringComparer.Ordinal)
                .ToDictionary(kv => kv.Key, kv => kv.Value);
            if (!vnpParams.TryGetValue("vnp_TxnRef", out var txnRef) || !vnpParams.TryGetValue("vnp_ResponseCode", out var responseCode))
                return false;

            var hashSecret = _config["Payment:VNPay:HashSecret"] ?? "";
            query.TryGetValue("vnp_SecureHash", out var vnpSecureHash);
            vnpSecureHash ??= "";
            if (string.IsNullOrEmpty(hashSecret))
            {
                message = "Chua cau hinh VNPay";
                return false;
            }

            var signData = SignData(vnpParams.Where(kv => kv.Key != "vnp_SecureHash" && kv.Key != "vnp_SecureHashType"));
            var myHash = HmacSha512(hashSecret, signData);
            if (!string.Equals(myHash, vnpSecureHash, StringComparison.OrdinalIgnoreCase))
            {
                message = "Chu ky VNPay khong hop le";
                return false;
            }

            var payment = _context.Payments.FirstOrDefault(p => p.GatewayOrderId == txnRef);
            if (payment == null) { message = "Khong tim thay giao dich"; return false; }
            if (payment.Status == PaymentStatus.Success) { bookingId = payment.BookingId; message = "Success"; return true; }

            if (query.TryGetValue("vnp_Amount", out var amountRaw)
                && long.TryParse(amountRaw, out var paidAmount)
                && paidAmount != (long)payment.Amount * 100)
            {
                message = "So tien khong khop";
                return false;
            }

            if (responseCode != "00")
            {
                payment.Status = PaymentStatus.Failed;
                payment.GatewayResponse = string.Join(";", query.Select(kv => $"{kv.Key}={kv.Value}"));
                _context.SaveChanges();
                message = "Thanh toan that bai hoac bi huy";
                bookingId = payment.BookingId;
                return false;
            }

            payment.Status = PaymentStatus.Success;
            payment.PaidAt = DateTime.UtcNow;
            payment.GatewayResponse = string.Join(";", query.Select(kv => $"{kv.Key}={kv.Value}"));
            var booking = _context.Bookings.Include(b => b.BookingSeats).FirstOrDefault(b => b.Id == payment.BookingId);
            if (booking != null && IsClosedBooking(booking.Status))
            {
                payment.Status = PaymentStatus.Cancelled;
                payment.GatewayResponse = "Đơn đã hết hạn thanh toán trước khi VNPay trả kết quả";
                _context.SaveChanges();
                message = "Don da het han thanh toan";
                bookingId = payment.BookingId;
                return false;
            }
            if (booking != null)
            {
                booking.Status = BookingStatus.Paid;
                CreateElectronicTicketsIfNotExists(booking);
            }
            _context.SaveChanges();
            bookingId = payment.BookingId;
            message = "Success";
            return true;
        }

        public bool HandleMomoCallback(IReadOnlyDictionary<string, string> query, out int bookingId, out string message)
        {
            bookingId = 0;
            message = "";
            query.TryGetValue("orderId", out var orderId);
            orderId ??= "";
            query.TryGetValue("resultCode", out var resultCode);
            resultCode ??= "";

            var payment = _context.Payments.FirstOrDefault(p => p.GatewayOrderId == orderId);
            if (payment == null)
            {
                message = "Khong tim thay giao dich";
                return false;
            }
            bookingId = payment.BookingId;
            if (payment.Status == PaymentStatus.Success)
            {
                message = "Success";
                return true;
            }
            if (!VerifyMomoSignature(query))
            {
                message = "Chu ky MoMo khong hop le";
                return false;
            }
            if (query.TryGetValue("amount", out var amountRaw)
                && long.TryParse(amountRaw, out var paidAmount)
                && paidAmount != (long)payment.Amount)
            {
                message = "So tien khong khop";
                return false;
            }

            payment.GatewayResponse = string.Join(";", query.Select(kv => $"{kv.Key}={kv.Value}"));
            if (resultCode != "0")
            {
                payment.Status = PaymentStatus.Failed;
                _context.SaveChanges();
                message = "Thanh toan MoMo that bai hoac bi huy";
                return false;
            }

            payment.Status = PaymentStatus.Success;
            payment.PaidAt = DateTime.UtcNow;
            var booking = _context.Bookings.Include(b => b.BookingSeats).FirstOrDefault(b => b.Id == payment.BookingId);
            if (booking != null && IsClosedBooking(booking.Status))
            {
                payment.Status = PaymentStatus.Cancelled;
                payment.GatewayResponse = "Đơn đã hết hạn thanh toán trước khi MoMo trả kết quả";
                _context.SaveChanges();
                message = "Don da het han thanh toan";
                return false;
            }
            if (booking != null)
            {
                booking.Status = BookingStatus.Paid;
                CreateElectronicTicketsIfNotExists(booking);
            }
            _context.SaveChanges();
            message = "Success";
            return true;
        }

        private bool VerifyMomoSignature(IReadOnlyDictionary<string, string> data)
        {
            var accessKey = _config["Payment:MoMo:AccessKey"] ?? "";
            var secretKey = _config["Payment:MoMo:SecretKey"] ?? "";
            if (string.IsNullOrWhiteSpace(accessKey) || string.IsNullOrWhiteSpace(secretKey)) return false;
            if (!data.TryGetValue("signature", out var signature) || string.IsNullOrWhiteSpace(signature)) return false;

            string Field(string key) => data.TryGetValue(key, out var value) ? value ?? "" : "";
            var raw =
                $"accessKey={accessKey}&amount={Field("amount")}&extraData={Field("extraData")}&message={Field("message")}&orderId={Field("orderId")}&orderInfo={Field("orderInfo")}&orderType={Field("orderType")}&partnerCode={Field("partnerCode")}&payType={Field("payType")}&requestId={Field("requestId")}&responseTime={Field("responseTime")}&resultCode={Field("resultCode")}&transId={Field("transId")}";
            var mine = HmacSha256(secretKey, raw);
            return string.Equals(mine, signature, StringComparison.OrdinalIgnoreCase);
        }

        public bool HandleStripeCallback(string sessionId, out int bookingId, out string message)
        {
            bookingId = 0;
            message = "";
            var payment = _context.Payments.FirstOrDefault(p => p.GatewayOrderId == sessionId);
            if (payment == null) { message = "Payment not found"; return false; }
            if (payment.Status == PaymentStatus.Success) { bookingId = payment.BookingId; message = "Already paid"; return true; }
            payment.Status = PaymentStatus.Success;
            payment.PaidAt = DateTime.UtcNow;
            payment.GatewayResponse = sessionId;
            var booking = _context.Bookings.Include(b => b.BookingSeats).FirstOrDefault(b => b.Id == payment.BookingId);
            if (booking != null && IsClosedBooking(booking.Status))
            {
                payment.Status = PaymentStatus.Cancelled;
                payment.GatewayResponse = "Đơn đã hết hạn thanh toán trước khi cổng trả kết quả";
                _context.SaveChanges();
                message = "Don da het han thanh toan";
                bookingId = payment.BookingId;
                return false;
            }
            if (booking != null)
            {
                booking.Status = BookingStatus.Paid;
                CreateElectronicTicketsIfNotExists(booking);
            }
            _context.SaveChanges();
            bookingId = payment.BookingId;
            message = "Success";
            return true;
        }

        private static bool IsClosedBooking(BookingStatus status)
        {
            return status is BookingStatus.Cancelled or BookingStatus.Expired
                or BookingStatus.Refunded or BookingStatus.RefundCompleted;
        }

        private void CreateElectronicTicketsIfNotExists(Booking booking)
        {
            var existing = _context.ElectronicTickets.Any(et => et.BookingId == booking.Id);
            if (existing) return;
            foreach (var bs in booking.BookingSeats ?? new List<BookingSeat>())
            {
                _context.ElectronicTickets.Add(new ElectronicTicket
                {
                    BookingId = booking.Id,
                    SeatId = bs.SeatId,
                    QRCode = $"BK{booking.Id}-S{bs.SeatId}-{Guid.NewGuid():N}"
                });
            }
        }

        private static DateTime VietnamNow()
        {
            var zone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zone);
        }

        private static string SignData(IEnumerable<KeyValuePair<string, string>> pairs)
        {
            return string.Join("&", pairs
                .Where(kv => !string.IsNullOrEmpty(kv.Value))
                .OrderBy(kv => kv.Key, StringComparer.Ordinal)
                .Select(kv => $"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value)}"));
        }

        private static string HmacSha512(string key, string data)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var dataBytes = Encoding.UTF8.GetBytes(data);
            using var hmac = new HMACSHA512(keyBytes);
            var hash = hmac.ComputeHash(dataBytes);
            return string.Concat(hash.Select(b => b.ToString("x2")));
        }

        private static string HmacSha256(string key, string data)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var dataBytes = Encoding.UTF8.GetBytes(data);
            using var hmac = new HMACSHA256(keyBytes);
            var hash = hmac.ComputeHash(dataBytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}
