namespace CinemaBooking.Application.DTO
{
    public class CreatePaymentUrlResponse
    {
        public int PaymentId { get; set; }
        public string PaymentUrl { get; set; } = "";
        public string? ClientSecret { get; set; } // Cho Stripe nếu dùng PaymentIntent
    }
}
