using CinemaBooking.Application.DTO;

namespace CinemaBooking.Application.UseCases.CreateBooking
{
    public class CreateBookingResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public CreateBookingResponse? Response { get; set; }

        public static CreateBookingResult Ok(CreateBookingResponse response) =>
            new() { Success = true, Response = response };

        public static CreateBookingResult Fail(string message) =>
            new() { Success = false, ErrorMessage = message };
    }
}
