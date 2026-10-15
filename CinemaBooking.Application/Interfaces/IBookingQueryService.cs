using CinemaBooking.Application.DTO;

namespace CinemaBooking.Application.Interfaces
{
    public interface IBookingQueryService
    {
        BookingDetailDto? GetById(int bookingId);
        List<BookingSummaryDto> GetBookings(int? userId = null);
    }
}
