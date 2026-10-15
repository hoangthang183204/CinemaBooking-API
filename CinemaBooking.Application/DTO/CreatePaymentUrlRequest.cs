using System.ComponentModel.DataAnnotations;

namespace CinemaBooking.Application.DTO
{
    public class CreatePaymentUrlRequest
    {
        [Required]
        public string Method { get; set; } = ""; // "momo", "vnpay", "stripe"
    }
}
