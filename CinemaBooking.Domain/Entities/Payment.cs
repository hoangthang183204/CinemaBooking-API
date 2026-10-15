using CinemaBooking.Domain.Enums;
using System;

namespace CinemaBooking.Domain.Entities
{
    public class Payment
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod Method { get; set; }
        public PaymentStatus Status { get; set; }
        public string? GatewayOrderId { get; set; }
        public string? GatewayResponse { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }

        // Navigation
        public Booking Booking { get; set; } = null!;
    }
}