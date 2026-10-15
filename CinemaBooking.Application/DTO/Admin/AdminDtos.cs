using System;

namespace CinemaBooking.Application.DTO.Admin
{
    // ========== DASHBOARD ==========
    public class DashboardStatsDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalBookings { get; set; }
        public int TotalUsers { get; set; }
        public int TotalMovies { get; set; }
        public int TotalRooms { get; set; }
        public int TotalShowTimes { get; set; }
        public int PaidBookings { get; set; }
        public int PendingBookings { get; set; }
        public List<RecentBookingDto> RecentBookings { get; set; } = new();
        public List<TopMovieDto> TopMovies { get; set; } = new();
    }

    public class RecentBookingDto
    {
        public int Id { get; set; }
        public string MovieTitle { get; set; } = "";
        public string UserEmail { get; set; } = "";
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }

    public class TopMovieDto
    {
        public int MovieId { get; set; }
        public string Title { get; set; } = "";
        public int BookingCount { get; set; }
        public decimal Revenue { get; set; }
    }

    // ========== MOVIE ==========
    public class AdminMovieDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public int Duration { get; set; }
        public bool IsActive { get; set; }
        public string? PosterUrl { get; set; }
        public string? Genre { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string? Description { get; set; }
        public string? Director { get; set; }
        public string? Country { get; set; }
        public string? Language { get; set; }
        public string? Cast { get; set; }
        public string? TrailerUrl { get; set; }
        public decimal? Rating { get; set; }
    }

    public class CreateUpdateMovieRequest
    {
        public string Title { get; set; } = "";
        public int Duration { get; set; }
        public bool IsActive { get; set; } = true;
        public string? PosterUrl { get; set; }
        public string? Genre { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string? Description { get; set; }
        public string? Director { get; set; }
        public string? Country { get; set; }
        public string? Language { get; set; }
        public string? Cast { get; set; }
        public string? TrailerUrl { get; set; }
        public decimal? Rating { get; set; }
    }

    // ========== ROOM ==========
    public class AdminRoomDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int SeatCount { get; set; }
        public int ShowTimeCount { get; set; }
    }

    public class CreateUpdateRoomRequest
    {
        public string Name { get; set; } = "";
        public int Rows { get; set; } = 5;          // số hàng ghế
        public int SeatsPerRow { get; set; } = 8;   // số ghế mỗi hàng
        public int CoupleRows { get; set; } = 1;    // số hàng cuối là ghế đôi
    }

    // ========== SHOWTIME ==========
    public class AdminShowTimeDto
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public string MovieTitle { get; set; } = "";
        public int RoomId { get; set; }
        public string RoomName { get; set; } = "";
        public DateTime StartTime { get; set; }
        public int BookedSeats { get; set; }
        public int TotalSeats { get; set; }
    }

    public class CreateUpdateShowTimeRequest
    {
        public int MovieId { get; set; }
        public int RoomId { get; set; }
        public DateTime StartTime { get; set; }
    }

    // ========== USER ==========
    public class AdminUserDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = "";
        public string FullName { get; set; } = "";
        public string? Phone { get; set; }
        public string Role { get; set; } = "";
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int BookingCount { get; set; }
    }

    public class UpdateUserRoleRequest
    {
        public string Role { get; set; } = "Customer";    // Customer / Staff / Admin
    }

    public class UpdateUserStatusRequest
    {
        public bool IsActive { get; set; }
    }

    // ========== BOOKING ==========
    public class AdminBookingDto
    {
        public int Id { get; set; }
        public string UserEmail { get; set; } = "";
        public string MovieTitle { get; set; } = "";
        public string RoomName { get; set; } = "";
        public DateTime ShowTimeStart { get; set; }
        public List<string> SeatNumbers { get; set; } = new();
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
}