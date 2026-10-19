using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaBooking.Application.DTO
{
    public class SeatStatusDto
    {
        public int SeatId { get; set; }
        public string SeatNumber { get; set; }
        public int SeatType { get; set; }
        public bool IsBooked { get; set; }
        public bool IsBlocked { get; set; }

        /// <summary>Người khác đang giữ ghế này.</summary>
        public bool IsHeld { get; set; }

        /// <summary>Chính user hiện tại đang giữ ghế này.</summary>
        public bool IsHeldByMe { get; set; }
    }
}
