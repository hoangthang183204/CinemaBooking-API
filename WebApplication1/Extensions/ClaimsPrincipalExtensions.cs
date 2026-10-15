using System.Security.Claims;

namespace CinemaBooking.API.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int? GetUserId(this ClaimsPrincipal user)
        {
            var claim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? user.FindFirst("sub")?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }

        public static string? GetEmail(this ClaimsPrincipal user)
            => user.FindFirst(ClaimTypes.Email)?.Value;

        public static bool IsAdmin(this ClaimsPrincipal user)
            => user.IsInRole("Admin");

        public static bool IsStaff(this ClaimsPrincipal user)
            => user.IsInRole("Staff");

        public static bool IsStaffOrAdmin(this ClaimsPrincipal user)
            => user.IsInRole("Staff") || user.IsInRole("Admin");
    }
}