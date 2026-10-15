using CinemaBooking.Application.DTO;

namespace CinemaBooking.Application.Interfaces
{
    public class SeatHoldResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public SeatHoldCreatedDto? Response { get; set; }

        public static SeatHoldResult Ok(SeatHoldCreatedDto response) =>
            new() { Success = true, Response = response };

        public static SeatHoldResult Fail(string message) =>
            new() { Success = false, ErrorMessage = message };
    }

    public interface ISeatHoldService
    {
        SeatHoldResult Hold(int showTimeId, int userId, IReadOnlyCollection<int> seatIds);
        bool Release(Guid holdToken, int userId);
        SeatAvailability GetAvailability(int showTimeId, int? userId);
    }

    public class SeatAvailability
    {
        public HashSet<int> OpenBookingSeatIds { get; set; } = new();
        public HashSet<int> HeldByOthers { get; set; } = new();
        public HashSet<int> HeldByUser { get; set; } = new();
    }
}
