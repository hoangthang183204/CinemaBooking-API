namespace CinemaBooking.Application.DTO
{
    public class CreateSeatHoldRequest
    {
        public List<int> SeatIds { get; set; } = new();
    }

    public class SeatHoldCreatedDto
    {
        public Guid HoldId { get; set; }
        public DateTime ExpiresAt { get; set; }
        public List<int> SeatIds { get; set; } = new();
    }
}
