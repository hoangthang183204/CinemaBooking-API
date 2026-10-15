using CinemaBooking.Domain.Entities;
using System.Collections.Generic;

namespace CinemaBooking.Application.Interfaces.Repositories
{
    public interface IRoomRepository
    {
        List<Room> GetAll();
        Room? GetById(int id);
    }
}
