using CinemaBooking.Application.DTO.Admin;
using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Enums;
using CinemaBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Infrastructure.Services
{
    internal static class CinemaPricing
    {
        public static readonly TimeZoneInfo Vietnam =
            TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        public static DateTime ToVietnam(DateTime value)
        {
            var utc = value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
                : value.ToUniversalTime();
            return TimeZoneInfo.ConvertTimeFromUtc(utc, Vietnam);
        }

        public static SystemSettingsDto LoadSettings(CinemaDbContext db)
        {
            var map = db.SystemSettings.AsNoTracking().ToDictionary(s => s.Key, s => s.Value);
            return new SystemSettingsDto
            {
                SeatPrice = DecimalOf(map, "SeatPrice", 75000m),
                OpenTime = TextOf(map, "OpenTime", "08:00"),
                CloseTime = TextOf(map, "CloseTime", "23:30"),
                WeekendSurcharge = DecimalOf(map, "WeekendSurcharge", 0m),
                LateShowSurcharge = DecimalOf(map, "LateShowSurcharge", 0m),
                LateShowAfter = TextOf(map, "LateShowAfter", "21:00"),
                LowStockAlert = (int)DecimalOf(map, "LowStockAlert", 10m)
            };
        }

        public static void SaveSettings(CinemaDbContext db, SystemSettingsDto dto)
        {
            Upsert(db, "SeatPrice", dto.SeatPrice.ToString("0.##"), "Giá vé mỗi ghế (đ)");
            Upsert(db, "OpenTime", NormalizeTime(dto.OpenTime, "08:00"), "Giờ mở cửa");
            Upsert(db, "CloseTime", NormalizeTime(dto.CloseTime, "23:30"), "Giờ đóng cửa");
            Upsert(db, "WeekendSurcharge", dto.WeekendSurcharge.ToString("0.##"), "Phụ thu cuối tuần mỗi ghế (đ)");
            Upsert(db, "LateShowSurcharge", dto.LateShowSurcharge.ToString("0.##"), "Phụ thu suất tối mỗi ghế (đ)");
            Upsert(db, "LateShowAfter", NormalizeTime(dto.LateShowAfter, "21:00"), "Suất tối tính từ giờ");
            Upsert(db, "LowStockAlert", Math.Max(0, dto.LowStockAlert).ToString(), "Cảnh báo bắp nước khi tồn dưới");
            db.SaveChanges();
        }

        public static string? OutsideOpenHours(CinemaDbContext db, DateTime startTime)
        {
            var settings = LoadSettings(db);
            var local = ToVietnam(startTime).TimeOfDay;
            if (!TimeSpan.TryParse(settings.OpenTime, out var open)) open = new TimeSpan(8, 0, 0);
            if (!TimeSpan.TryParse(settings.CloseTime, out var close)) close = new TimeSpan(23, 30, 0);
            if (local < open || local > close)
                return $"Giờ chiếu phải trong khung mở cửa {settings.OpenTime}–{settings.CloseTime}.";
            return null;
        }

        public static QuotePromotionDto Quote(
            CinemaDbContext db,
            int showtimeId,
            int seatCount,
            decimal snackTotal,
            string? code,
            bool consume,
            int? userId = null,
            int? exceptBookingId = null)
        {
            var settings = LoadSettings(db);
            var show = db.ShowTimes.AsNoTracking().FirstOrDefault(s => s.Id == showtimeId);
            var local = show == null ? ToVietnam(DateTime.UtcNow) : ToVietnam(show.StartTime);
            var surchargePerSeat = 0m;
            if (local.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                surchargePerSeat += settings.WeekendSurcharge;
            if (TimeSpan.TryParse(settings.LateShowAfter, out var lateAfter) && local.TimeOfDay >= lateAfter)
                surchargePerSeat += settings.LateShowSurcharge;

            var ticket = settings.SeatPrice * Math.Max(0, seatCount);
            var surcharge = surchargePerSeat * Math.Max(0, seatCount);
            var snacks = Math.Max(0, snackTotal);
            var subtotal = ticket + surcharge + snacks;

            Promotion? promo = null;
            if (!string.IsNullOrWhiteSpace(code))
            {
                var needle = code.Trim().ToUpperInvariant();
                promo = db.Promotions.FirstOrDefault(p => p.Code != null && p.Code.ToUpper() == needle);
                if (promo == null)
                    return Fail(settings, ticket, surcharge, snacks, "Mã giảm giá không tồn tại.");
            }
            else
            {
                promo = db.Promotions
                    .Where(p => p.IsActive && p.Kind == "Weekday" && p.Weekday == (int)local.DayOfWeek)
                    .OrderByDescending(p => p.Value)
                    .FirstOrDefault();
            }

            var discount = 0m;
            string? name = promo?.Name;
            string? usedCode = promo?.Code;
            if (promo != null)
            {
                var reused = SingleUse(db, userId, promo.Code, exceptBookingId);
                if (reused != null)
                    return Fail(settings, ticket, surcharge, snacks, reused);

                var check = Validate(promo, local, subtotal);
                if (check != null)
                    return Fail(settings, ticket, surcharge, snacks, check);

                var baseAmount = promo.AppliesTo switch
                {
                    "Ticket" => ticket + surcharge,
                    "Concession" => snacks,
                    _ => subtotal
                };
                discount = promo.Kind == "Fixed"
                    ? promo.Value
                    : Math.Round(baseAmount * promo.Value / 100m, 0, MidpointRounding.AwayFromZero);
                if (promo.MaxDiscount is decimal cap && cap > 0 && discount > cap)
                    discount = cap;
                if (discount > subtotal) discount = subtotal;

                if (consume && discount > 0)
                {
                    promo.UsedCount += 1;
                }
            }

            return new QuotePromotionDto
            {
                Ok = true,
                Message = discount > 0 ? $"Đã áp dụng {name}." : "Không có mã giảm.",
                Name = name,
                Code = usedCode,
                SeatPrice = settings.SeatPrice,
                TicketSubtotal = ticket,
                Surcharge = surcharge,
                SnackTotal = snacks,
                Discount = discount,
                Total = Math.Max(0, subtotal - discount)
            };
        }

        private static QuotePromotionDto Fail(
            SystemSettingsDto settings, decimal ticket, decimal surcharge, decimal snacks, string message)
        {
            return new QuotePromotionDto
            {
                Ok = false,
                Message = message,
                SeatPrice = settings.SeatPrice,
                TicketSubtotal = ticket,
                Surcharge = surcharge,
                SnackTotal = snacks,
                Discount = 0,
                Total = ticket + surcharge + snacks
            };
        }

        private static string? Validate(Promotion promo, DateTime local, decimal subtotal)
        {
            if (!promo.IsActive) return "Chương trình đã tắt.";
            if (promo.StartsAt != null && local < ToVietnam(promo.StartsAt.Value))
                return "Chương trình chưa bắt đầu.";
            if (promo.EndsAt != null && local > ToVietnam(promo.EndsAt.Value))
                return "Chương trình đã hết hạn.";
            if (promo.Kind == "Weekday" && promo.Weekday != null && promo.Weekday != (int)local.DayOfWeek)
                return "Mã này chỉ dùng đúng ngày trong tuần đã cấu hình.";
            if (promo.MinSubtotal > 0 && subtotal < promo.MinSubtotal)
                return $"Đơn tối thiểu {promo.MinSubtotal:N0}đ mới dùng được mã này.";
            var limit = promo.UsageLimit ?? 1;
            if (promo.UsedCount >= limit)
                return "Mã đã hết lượt sử dụng.";
            return null;
        }

        private static string? SingleUse(CinemaDbContext db, int? userId, string? code, int? exceptBookingId = null)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;
            var needle = code.Trim().ToUpperInvariant();
            var rows = db.Bookings.Where(b =>
                b.PromoCode != null
                && b.PromoCode.ToUpper() == needle
                && (exceptBookingId == null || b.Id != exceptBookingId)
                && b.Status != BookingStatus.Cancelled
                && b.Status != BookingStatus.Expired
                && b.Status != BookingStatus.Failed);
            if (userId is int uid && uid > 0 && rows.Any(b => b.UserId == uid))
                return "Bạn đã dùng mã này rồi. Mỗi mã chỉ dùng 1 lần.";
            if (rows.Any())
                return "Mã đã hết lượt sử dụng.";
            return null;
        }

        private static void Upsert(CinemaDbContext db, string key, string value, string label)
        {
            var row = db.SystemSettings.FirstOrDefault(s => s.Key == key);
            if (row == null)
            {
                db.SystemSettings.Add(new SystemSetting { Key = key, Value = value, Label = label });
                return;
            }
            row.Value = value;
            row.Label = label;
        }

        private static decimal DecimalOf(Dictionary<string, string> map, string key, decimal fallback)
        {
            if (!map.TryGetValue(key, out var text)) return fallback;
            return decimal.TryParse(text, out var n) ? n : fallback;
        }

        private static string TextOf(Dictionary<string, string> map, string key, string fallback)
            => map.TryGetValue(key, out var text) && !string.IsNullOrWhiteSpace(text) ? text.Trim() : fallback;

        private static string NormalizeTime(string? value, string fallback)
        {
            if (TimeSpan.TryParse(value, out var span))
                return span.ToString(@"hh\:mm");
            return fallback;
        }
    }
}
