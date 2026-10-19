using System;

namespace CinemaBooking.Application.Common
{
    public static class ShowTimeClock
    {
        public static readonly TimeZoneInfo Vietnam =
            TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        /// <summary>
        /// Suất chiếu lưu giờ tường Việt Nam (không có múi). Đổi sang UTC để so với UtcNow.
        /// </summary>
        public static DateTime AsUtc(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => TimeZoneInfo.ConvertTimeToUtc(value, Vietnam)
            };
        }

        public static readonly TimeSpan CancelCutoff = TimeSpan.FromHours(2);

        public static bool HasStarted(DateTime startTime, DateTime? utcNow = null)
        {
            var now = utcNow ?? DateTime.UtcNow;
            return AsUtc(startTime) <= now;
        }

        public static bool CanCustomerCancel(DateTime startTime, DateTime? utcNow = null)
        {
            var now = utcNow ?? DateTime.UtcNow;
            return AsUtc(startTime) - now >= CancelCutoff;
        }
    }
}
