namespace CinemaBooking.Application.Common
{
    public static class ShowTimeClock
    {
        public static DateTime AsUtc(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
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
