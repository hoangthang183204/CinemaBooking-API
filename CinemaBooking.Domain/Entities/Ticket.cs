namespace CinemaBooking.Domain.Entities
{
    public class Ticket
    {
        public int Id { get; set; }
        public int ShowTimeId { get; set; }
        public int SeatId { get; set; }
        public decimal Price { get; set; }
    }
}