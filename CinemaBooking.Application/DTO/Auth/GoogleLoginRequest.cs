using System.ComponentModel.DataAnnotations;

namespace CinemaBooking.Application.DTO.Auth
{
    public class GoogleLoginRequest
    {
        [Required]
        public string IdToken { get; set; } = "";
    }
}
