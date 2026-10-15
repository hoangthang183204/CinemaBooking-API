using CinemaBooking.Application.Common;
using CinemaBooking.Application.DTO;
using CinemaBooking.Application.UseCases.BookTicket;
using Microsoft.AspNetCore.Mvc;

namespace CinemaBooking.API.Controllers
{
    [ApiController]
    [Route("api/tickets")]
    public class TicketsController : ControllerBase
    {
        private readonly BookTicketUseCase _bookTicketUseCase;

        public TicketsController(BookTicketUseCase bookTicketUseCase)
        {
            _bookTicketUseCase = bookTicketUseCase;
        }

        [HttpPost]
        public IActionResult BookTicket([FromBody] BookTicketRequest request)
        {
            var response = _bookTicketUseCase.Execute(request);
            if (response.TicketId == 0)
                return BadRequest(ApiResponse<object>.Fail(response.Message ?? "Ghế đã được đặt"));

            return Ok(ApiResponse<BookTicketResponse>.Ok(response, response.Message ?? "Đặt vé thành công"));
        }
    }
}
