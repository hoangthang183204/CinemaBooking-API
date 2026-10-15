using CinemaBooking.Domain.Entities;
using System.Collections.Generic;

namespace CinemaBooking.Application.Interfaces.Repositories
{
    public interface ISeatRepository
    {
        Seat? GetById(int seatId);
        List<Seat> GetByRoomId(int roomId);
    }
}
