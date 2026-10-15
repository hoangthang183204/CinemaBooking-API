using CinemaBooking.Application.DTO;
using CinemaBooking.Application.Interfaces.Repositories;
using CinemaBooking.Domain.Entities;

namespace CinemaBooking.Application.Services
{
    public class TicketService
    {
        private readonly ITicketRepository _ticketRepo;

        public TicketService(ITicketRepository ticketRepo)
        {
            _ticketRepo = ticketRepo;
        }

        public bool BookTicket(BookTicketDto dto)
        {
            if (_ticketRepo.IsSeatBooked(dto.ShowTimeId, dto.SeatId))
                return false;

            var ticket = new Ticket
            {
                ShowTimeId = dto.ShowTimeId,
                SeatId = dto.SeatId,
                Price = 90000 // tạm
            };

            _ticketRepo.Add(ticket);
            return true;
        }

    }
}
