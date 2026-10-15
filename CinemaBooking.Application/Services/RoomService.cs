using CinemaBooking.Application.Interfaces.Repositories;
using CinemaBooking.Domain.Entities;

namespace CinemaBooking.Application.Services
{
    public class RoomService
    {
        private readonly IRoomRepository _roomRepository;

        public RoomService(IRoomRepository roomRepository)
        {
            _roomRepository = roomRepository;
        }

        public List<Room> GetRooms()
        {
            return _roomRepository.GetAll();
        }

        public Room? GetById(int id)
        {
            return _roomRepository.GetById(id);
        }
    }
}
