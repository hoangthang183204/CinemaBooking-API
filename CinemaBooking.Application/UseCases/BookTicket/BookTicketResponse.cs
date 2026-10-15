using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaBooking.Application.UseCases.BookTicket
{
    public class BookTicketResponse
    {
        public int TicketId { get; set; }
        public decimal Price { get; set; }
        public string Message { get; set; }
    }
}
