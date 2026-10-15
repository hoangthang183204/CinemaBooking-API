using System.ComponentModel.DataAnnotations;

namespace CinemaBooking.Application.DTO.Auth
{
    public class LoginRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string Password { get; set; } = "";
    }
}