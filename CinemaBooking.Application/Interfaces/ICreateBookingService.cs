using CinemaBooking.Application.DTO;
using CinemaBooking.Application.UseCases.CreateBooking;

namespace CinemaBooking.Application.Interfaces
{
    public interface ICreateBookingService
    {
        CreateBookingResult Execute(CreateBookingRequest request);
    }
}
