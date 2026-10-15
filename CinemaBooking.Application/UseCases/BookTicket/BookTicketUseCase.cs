using CinemaBooking.Application.Interfaces.Repositories;
using CinemaBooking.Domain.Entities;

namespace CinemaBooking.Application.UseCases.BookTicket
{
    public class BookTicketUseCase
    {
        private readonly ITicketRepository _ticketRepository;

        public BookTicketUseCase(ITicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }

        public BookTicketResponse Execute(BookTicketRequest request)
        {
            // 1. Kiểm tra ghế đã được đặt chưa
            if (_ticketRepository.IsSeatBooked(request.ShowTimeId, request.SeatId))
            {
                return new BookTicketResponse
                {
                    Message = "Ghế đã được đặt"
                };
            }

            // 2. Tạo vé
            var ticket = new Ticket
            {
                ShowTimeId = request.ShowTimeId,
                SeatId = request.SeatId,
                Price = 75000
            };

            // 3. Lưu vé
            _ticketRepository.Add(ticket);

            // 4. Trả kết quả
            return new BookTicketResponse
            {
                TicketId = ticket.Id,   // EF sẽ tự gán sau SaveChanges
                Price = ticket.Price,
                Message = "Đặt vé thành công"
            };
        }
    }
}
