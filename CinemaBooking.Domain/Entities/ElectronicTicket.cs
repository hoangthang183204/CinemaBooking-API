namespace CinemaBooking.Domain.Entities
{
    /// <summary>
    /// Vé điện tử (1 vé = 1 ghế trong 1 booking đã thanh toán).
    /// </summary>
    public class ElectronicTicket
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public int SeatId { get; set; }
        public string QRCode { get; set; } = "";

        // Navigation — CHỈ 1 nav cho mỗi FK
        public Booking Booking { get; set; } = null!;
        public Seat Seat { get; set; } = null!;
    }
}