using System;

namespace CinemaBooking.Domain.Entities
{
    public class StaffShift
    {
        public int Id { get; set; }
        public int StaffId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public decimal? ExpectedCash { get; set; }
        public decimal? ExpectedTransfer { get; set; }
        public decimal? CountedCash { get; set; }
        public string? Note { get; set; }
        public string? ReportJson { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public int? ApprovedById { get; set; }
        public int? AssignedById { get; set; }
        public decimal? OpeningCash { get; set; }
        public decimal? HandoverCash { get; set; }
        public bool AwaitingHandover { get; set; }
        public DateTime? ReceivedAt { get; set; }
        public int? ReceivedById { get; set; }
    }
}
