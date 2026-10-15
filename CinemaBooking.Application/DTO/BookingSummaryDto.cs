using System;
using System.Collections.Generic;

namespace CinemaBooking.Application.DTO
{
    public class BookingSummaryDto
    {
        public int Id { get; set; }
        public string MovieTitle { get; set; } = "";
        public string RoomName { get; set; } = "";
        public DateTime ShowTimeStart { get; set; }
        public List<string> SeatNumbers { get; set; } = new();
        public string Status { get; set; } = "";
        public decimal TotalPrice { get; set; }
    }
}
