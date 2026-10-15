using CinemaBooking.Application.DTO.Admin;
using CinemaBooking.Application.DTO.Staff;

namespace CinemaBooking.Application.Interfaces
{
    public interface IAdminService
    {
        // Dashboard
        DashboardStatsDto GetDashboardStats();

        // Movies
        List<AdminMovieDto> GetAllMovies();
        AdminMovieDto? GetMovieById(int id);
        AdminMovieDto CreateMovie(CreateUpdateMovieRequest request);
        AdminMovieDto? UpdateMovie(int id, CreateUpdateMovieRequest request);
        bool DeleteMovie(int id);

        // Rooms
        List<AdminRoomDto> GetAllRooms();
        AdminRoomDto? GetRoomById(int id);
        AdminRoomDto CreateRoom(CreateUpdateRoomRequest request);
        AdminRoomDto? UpdateRoom(int id, CreateUpdateRoomRequest request);
        bool DeleteRoom(int id);

        // ShowTimes
        List<AdminShowTimeDto> GetAllShowTimes();
        AdminShowTimeDto? GetShowTimeById(int id);
        AdminShowTimeDto? CreateShowTime(CreateUpdateShowTimeRequest request);
        AdminShowTimeDto? UpdateShowTime(int id, CreateUpdateShowTimeRequest request);
        bool DeleteShowTime(int id);

        // Users
        List<AdminUserDto> GetAllUsers();
        AdminUserDto? GetUserById(int id);
        AdminUserDto? UpdateUserRole(int id, string role);
        AdminUserDto? UpdateUserStatus(int id, bool isActive);

        // Bookings
        List<AdminBookingDto> GetAllBookings();
        AdminBookingDto? GetBookingById(int id);
        StaffBookingDetailDto? GetBookingDetail(int id);
        bool CancelBooking(int id);
        (bool Ok, string Message) CancelForCustomer(int id, int userId);
        bool MarkBookingAsPaid(int id);
        bool CompleteRefund(int id);
        bool TryConfirmBankTransfer(string? content, decimal amount);


        CounterBookingResponse? CreateCounterBooking(
            CreateCounterBookingRequest request,
            int staffId);

        // ========== STAFF: XÁC NHẬN THANH TOÁN ==========
        ConfirmPaymentResponse? ConfirmBookingPayment(
            int bookingId,
            int staffId,
            string? note);

        // ========== STAFF: CHECK-IN QR ==========
        CheckInResponse? CheckInByQRCode(string qrCode);
    }
}