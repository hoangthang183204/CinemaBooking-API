using System;

namespace CinemaBooking.Domain.Entities
{
    public class ShowTime
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }

        public int MovieId { get; set; }
        public int RoomId { get; set; }

        public Movie Movie { get; set; } = null!;
        public Room Room { get; set; } = null!;
    }
}