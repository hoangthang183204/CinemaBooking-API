namespace CinemaBooking.Domain.Entities
{
    public class BookingSeat
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public int SeatId { get; set; }
        public decimal Price { get; set; }

        // Navigation
        public Booking Booking { get; set; } = null!;
        public Seat Seat { get; set; } = null!;
    }
}