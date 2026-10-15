namespace CinemaBooking.Application.DTO
{
    public class CreateBookingResponse
    {
        public int BookingId { get; set; }
        public decimal TotalPrice { get; set; }
        public string Message { get; set; } = "Đặt vé thành công.";
    }
}
