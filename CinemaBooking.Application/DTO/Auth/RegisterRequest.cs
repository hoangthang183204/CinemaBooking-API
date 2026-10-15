using System.ComponentModel.DataAnnotations;

namespace CinemaBooking.Application.DTO.Auth
{
    public class RegisterRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, MinLength(6)]
        public string Password { get; set; } = "";

        [Required, MaxLength(128)]
        public string FullName { get; set; } = "";

        [Phone]
        public string? Phone { get; set; }
    }
}