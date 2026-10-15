using CinemaBooking.Domain.Enums;
using System;
using System.Collections.Generic;

namespace CinemaBooking.Domain.Entities
{
    public class Booking
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public int ShowTimeId { get; set; }
        public decimal TotalPrice { get; set; }
        public BookingStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation
        public ShowTime ShowTime { get; set; } = null!;
        public ICollection<BookingSeat> BookingSeats { get; set; } = new List<BookingSeat>();
        public ICollection<BookingConcession> Concessions { get; set; } = new List<BookingConcession>();
        public ICollection<ElectronicTicket> ElectronicTickets { get; set; } = new List<ElectronicTicket>();
    }
}