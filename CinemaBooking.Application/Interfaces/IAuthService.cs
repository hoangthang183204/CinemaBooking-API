using CinemaBooking.Application.DTO.Auth;

namespace CinemaBooking.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse?> LoginAsync(LoginRequest request);
        Task<AuthResponse?> GetCurrentUserAsync(int userId);
    }
}