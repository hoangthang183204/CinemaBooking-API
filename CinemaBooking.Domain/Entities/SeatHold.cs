namespace CinemaBooking.Domain.Entities
{
    /// <summary>
    /// Giữ ghế tạm cho một suất. Hết hạn thì xóa dòng, ghế trở lại trống.
    /// Một dòng = một ghế. Các ghế của cùng một lần giữ dùng chung HoldToken.
    /// </summary>
    public class SeatHold
    {
        public int Id { get; set; }
        public int ShowTimeId { get; set; }
        public int SeatId { get; set; }
        public int UserId { get; set; }
        public Guid HoldToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public ShowTime ShowTime { get; set; } = null!;
        public Seat Seat { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
