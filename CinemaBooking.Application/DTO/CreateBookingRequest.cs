using System.ComponentModel.DataAnnotations;

namespace CinemaBooking.Application.DTO
{
    public class CreateBookingRequest
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int ShowtimeId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Chọn ít nhất một ghế")]
        public List<int> SeatIds { get; set; } = new();

        public List<ConcessionPickDto> Concessions { get; set; } = new();
        public string? PromoCode { get; set; }

        // Server tự gán từ JWT — client không cần gửi
        public int? UserId { get; set; }
    }

    public class ConcessionPickDto
    {
        public int ConcessionId { get; set; }
        public int Quantity { get; set; }
    }
}