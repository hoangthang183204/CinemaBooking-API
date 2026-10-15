using System;

namespace CinemaBooking.Application.DTO
{
    public class ShowTimeDto
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public int MovieId { get; set; }
        public string MovieTitle { get; set; } = string.Empty;
        public int RoomId { get; set; }
        public string RoomName { get; set; } = string.Empty;
    }
}
