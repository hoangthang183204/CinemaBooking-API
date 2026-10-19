using System;

namespace CinemaBooking.Domain.Entities
{
    public class Promotion
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Code { get; set; }
        /// <summary>Percent, Fixed, Weekday</summary>
        public string Kind { get; set; } = "Percent";
        public decimal Value { get; set; }
        /// <summary>0 = Chủ nhật … 6 = Thứ bảy (.NET DayOfWeek).</summary>
        public int? Weekday { get; set; }
        public decimal MinSubtotal { get; set; }
        public decimal? MaxDiscount { get; set; }
        public int? UsageLimit { get; set; }
        public int UsedCount { get; set; }
        public DateTime? StartsAt { get; set; }
        public DateTime? EndsAt { get; set; }
        /// <summary>All, Ticket, Concession</summary>
        public string AppliesTo { get; set; } = "All";
        public string? Note { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
