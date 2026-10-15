using CinemaBooking.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaBooking.Domain.Entities
{
    public class Seat
    {
        public int Id { get; set; }
        public string SeatNumber { get; set; } = "";
        public SeatType SeatType { get; set; }
        public int RoomId { get; set; }
        public Room Room { get; set; } = null!;

        // Runtime state — KHÔNG lưu vào DB
        [NotMapped]
        public bool IsBooked { get; set; }
    }
}