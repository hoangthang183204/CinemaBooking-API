using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Enums;

namespace CinemaBooking.Application.Interfaces.Repositories
{
    public interface ITicketRepository
    {
        List<int> GetBookedSeatIds(int showTimeId, TicketStatus status);
        bool IsSeatBooked(int showTimeId, int seatId);
        void Add(Ticket ticket);

    }
}
