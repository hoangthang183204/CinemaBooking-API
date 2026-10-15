using CinemaBooking.Domain.Enums;
using System;

namespace CinemaBooking.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public string FullName { get; set; } = "";
        public string? Phone { get; set; }
        public UserRole Role { get; set; } = UserRole.Customer;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}