using System;

namespace CinemaBooking.API.Models
{
    public class CreateShowTimeRequest
    {
        public int MovieId { get; set; }
        public int RoomId { get; set; }
        public DateTime StartTime { get; set; }
    }
}
