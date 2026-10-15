namespace CinemaBooking.Domain.Entities
{
    public class BookingConcession
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public int ConcessionItemId { get; set; }
        public string ItemName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public Booking Booking { get; set; } = null!;
        public ConcessionItem ConcessionItem { get; set; } = null!;
    }
}
