using System.ComponentModel.DataAnnotations;

namespace CinemaBooking.Application.UseCases.BookTicket
{
    public class BookTicketRequest
    {
        [Required(ErrorMessage = "ShowTimeId là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "ShowTimeId phải lớn hơn 0")]
        public int ShowTimeId { get; set; }

        [Required(ErrorMessage = "SeatId là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "SeatId phải lớn hơn 0")]
        public int SeatId { get; set; }
    }
}
