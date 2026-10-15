using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaBooking.Application.DTO
{
    public class RoomSeatDto
    {
        public int RoomId { get; set; }
        public string RoomName { get; set; }
        public List<SeatStatusDto> Seats { get; set; }
    }
}
