using System.Collections.Generic;
using CinemaBooking.Application.DTO;

namespace CinemaBooking.Application.Interfaces
{
    public interface IPaymentService
    {
        CreatePaymentUrlResponse? CreatePaymentUrl(int bookingId, CreatePaymentUrlRequest request, string returnBaseUrl, string ipAddress);
        bool HandleVnPayCallback(IReadOnlyDictionary<string, string> query, out int bookingId, out string message);
        bool HandleMomoCallback(IReadOnlyDictionary<string, string> query, out int bookingId, out string message);
        bool HandleStripeCallback(string sessionId, out int bookingId, out string message);
    }
}
