using CinemaBooking.Application.Interfaces.Repositories;
using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Enums;


namespace CinemaBooking.Application.Services
{
    public class SeatService
    {
        private readonly ISeatRepository _seatRepository;
        private readonly ITicketRepository _ticketRepository;

        public SeatService(ISeatRepository seatRepository, ITicketRepository ticketRepository)
        {
            _seatRepository = seatRepository;
            _ticketRepository = ticketRepository;
        }

        public List<Seat> GetSeatsByRoom(int roomId, int showTimeId)
        {
            var seats = _seatRepository.GetByRoomId(roomId);

            var bookedSeatIds = _ticketRepository.GetBookedSeatIds(showTimeId, TicketStatus.Booked);

            foreach (var seat in seats)
            {
                seat.IsBooked = bookedSeatIds.Contains(seat.Id);
            }

            return seats;
        }
    }
}
