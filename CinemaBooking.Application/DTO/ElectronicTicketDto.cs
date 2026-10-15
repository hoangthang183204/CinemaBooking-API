namespace CinemaBooking.Application.DTO
{
    public class ElectronicTicketDto
    {
        public int Id { get; set; }
        public int SeatId { get; set; }
        public string SeatNumber { get; set; } = "";
        public string QRCode { get; set; } = "";
    }
}
