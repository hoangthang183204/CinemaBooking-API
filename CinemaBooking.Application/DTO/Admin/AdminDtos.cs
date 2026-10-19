using System;
using CinemaBooking.Application.DTO.Staff;

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
        public List<RevenuePointDto> RevenueByDay { get; set; } = new();
        public decimal OccupancyRate { get; set; }
        public List<RoomOccupancyDto> RoomOccupancy { get; set; } = new();
        public List<AlertDto> Alerts { get; set; } = new();
        public int RefundPending { get; set; }
    }

    public class RevenuePointDto
    {
        public string Date { get; set; } = "";
        public decimal Revenue { get; set; }
        public int Bookings { get; set; }
        public List<RevenueLineDto> Lines { get; set; } = new();
    }

    public class RevenueLineDto
    {
        public int BookingId { get; set; }
        public string MovieTitle { get; set; } = "";
        public string RoomName { get; set; } = "";
        public DateTime ShowTimeStart { get; set; }
        public int Seats { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class RoomOccupancyDto
    {
        public int RoomId { get; set; }
        public string RoomName { get; set; } = "";
        public int Capacity { get; set; }
        public int SoldSeats { get; set; }
        public decimal Rate { get; set; }
        public List<ShowFillDto> Shows { get; set; } = new();
    }

    public class ShowFillDto
    {
        public int ShowTimeId { get; set; }
        public string MovieTitle { get; set; } = "";
        public DateTime StartTime { get; set; }
        public int Sold { get; set; }
        public int Capacity { get; set; }
        public decimal Rate { get; set; }
    }

    public class AlertDto
    {
        public string Level { get; set; } = "info";
        public string Title { get; set; } = "";
        public string Text { get; set; } = "";
        public string? Href { get; set; }
    }

    public class AdminSeatDto
    {
        public int Id { get; set; }
        public string SeatNumber { get; set; } = "";
        public int SeatType { get; set; }
        public bool IsBlocked { get; set; }
    }

    public class RoomLayoutDto
    {
        public int RoomId { get; set; }
        public string RoomName { get; set; } = "";
        public List<AdminSeatDto> Seats { get; set; } = new();
    }

    public class UpdateSeatLayoutRequest
    {
        public List<AdminSeatDto> Seats { get; set; } = new();
    }

    public class PromotionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Code { get; set; }
        public string Kind { get; set; } = "Percent";
        public decimal Value { get; set; }
        public int? Weekday { get; set; }
        public decimal MinSubtotal { get; set; }
        public decimal? MaxDiscount { get; set; }
        public int? UsageLimit { get; set; }
        public int UsedCount { get; set; }
        public DateTime? StartsAt { get; set; }
        public DateTime? EndsAt { get; set; }
        public string AppliesTo { get; set; } = "All";
        public string? Note { get; set; }
        public bool IsActive { get; set; }
    }

    public class SavePromotionRequest
    {
        public string Name { get; set; } = "";
        public string? Code { get; set; }
        public string Kind { get; set; } = "Percent";
        public decimal Value { get; set; }
        public int? Weekday { get; set; }
        public decimal MinSubtotal { get; set; }
        public decimal? MaxDiscount { get; set; }
        public int? UsageLimit { get; set; }
        public DateTime? StartsAt { get; set; }
        public DateTime? EndsAt { get; set; }
        public string AppliesTo { get; set; } = "All";
        public string? Note { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class QuotePromotionRequest
    {
        public string? Code { get; set; }
        public int? UserId { get; set; }
        public int ShowtimeId { get; set; }
        public int SeatCount { get; set; }
        public decimal SnackTotal { get; set; }
    }

    public class QuotePromotionDto
    {
        public bool Ok { get; set; }
        public string Message { get; set; } = "";
        public string? Name { get; set; }
        public string? Code { get; set; }
        public decimal SeatPrice { get; set; }
        public decimal TicketSubtotal { get; set; }
        public decimal Surcharge { get; set; }
        public decimal SnackTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
    }

    public class ReportQuery
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public string GroupBy { get; set; } = "movie";
    }

    public class ReportDto
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public string GroupBy { get; set; } = "movie";
        public decimal TotalRevenue { get; set; }
        public int TotalBookings { get; set; }
        public int TotalSeats { get; set; }
        public List<ReportRowDto> Rows { get; set; } = new();
    }

    public class ReportRowDto
    {
        public string Label { get; set; } = "";
        public int Bookings { get; set; }
        public int Seats { get; set; }
        public decimal Revenue { get; set; }
    }

    public class SystemSettingsDto
    {
        public decimal SeatPrice { get; set; }
        public string OpenTime { get; set; } = "08:00";
        public string CloseTime { get; set; } = "23:30";
        public decimal WeekendSurcharge { get; set; }
        public decimal LateShowSurcharge { get; set; }
        public string LateShowAfter { get; set; } = "21:00";
        public int LowStockAlert { get; set; } = 10;
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
        public string? CitizenId { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Position { get; set; }
    }

    public class StaffProfileDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? Phone { get; set; }
        public string? CitizenId { get; set; }
        public string? AvatarUrl { get; set; }
        public string Position { get; set; } = "";
        public string Role { get; set; } = "";
        public bool IsActive { get; set; }
        public int ShiftCount { get; set; }
        public decimal Revenue { get; set; }
        public bool OnDuty { get; set; }
        public bool HandoverPending { get; set; }
        public DateTime? DutyStartedAt { get; set; }
        public List<AdminShiftDto> Shifts { get; set; } = new();
    }

    public class SaveStaffRequest
    {
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? Phone { get; set; }
        public string? CitizenId { get; set; }
        public string? AvatarUrl { get; set; }
        public string Position { get; set; } = "TicketSeller";
        public string? Password { get; set; }
        public bool IsActive { get; set; } = true;
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
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string MovieTitle { get; set; } = "";
        public string RoomName { get; set; } = "";
        public DateTime ShowTimeStart { get; set; }
        public List<string> SeatNumbers { get; set; } = new();
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
}