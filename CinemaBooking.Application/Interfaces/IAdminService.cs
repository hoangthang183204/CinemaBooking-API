using CinemaBooking.Application.DTO.Admin;
using CinemaBooking.Application.DTO.Staff;

namespace CinemaBooking.Application.Interfaces
{
    public interface IAdminService
    {
        // Dashboard
        DashboardStatsDto GetDashboardStats();

        RoomLayoutDto? GetRoomLayout(int roomId);
        RoomLayoutDto? SaveRoomLayout(int roomId, UpdateSeatLayoutRequest request);

        List<PromotionDto> GetPromotions();
        PromotionDto? GetPromotion(int id);
        PromotionDto CreatePromotion(SavePromotionRequest request);
        PromotionDto? UpdatePromotion(int id, SavePromotionRequest request);
        bool DeletePromotion(int id);
        QuotePromotionDto QuotePromotion(QuotePromotionRequest request);

        ReportDto GetReport(DateTime from, DateTime to, string groupBy);
        byte[] ExportReportExcel(DateTime from, DateTime to, string groupBy);

        SystemSettingsDto GetSettings();
        SystemSettingsDto SaveSettings(SystemSettingsDto request);

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
        List<StaffProfileDto> GetStaffDirectory();
        StaffProfileDto? GetStaffProfile(int id);
        StaffProfileDto CreateStaff(SaveStaffRequest request);
        StaffProfileDto? UpdateStaff(int id, SaveStaffRequest request);

        // Bookings
        List<AdminBookingDto> GetAllBookings();
        AdminBookingDto? GetBookingById(int id);
        StaffBookingDetailDto? GetBookingDetail(int id);
        bool CancelBooking(int id);
        (bool Ok, string Message) CancelForCustomer(int id, int userId);
        bool MarkBookingAsPaid(int id, int staffId = 0);
        bool CompleteRefund(int id);
        bool TryConfirmBankTransfer(string? content, decimal amount);


        CounterBookingResponse? CreateCounterBooking(
            CreateCounterBookingRequest request,
            int staffId);

        // ========== STAFF: XÁC NHẬN THANH TOÁN ==========
        ConfirmPaymentResponse? ConfirmBookingPayment(
            int bookingId,
            int staffId,
            string? note,
            string? method = null,
            decimal? cashReceived = null);

        // ========== STAFF: CHECK-IN QR ==========
        CheckInResponse? CheckInByQRCode(string qrCode);

        List<AdminShiftDto> GetShifts();
        AdminShiftDto? GetShift(int id);
        AdminShiftDto AssignShift(int staffId, int adminId, string? note);
        AdminShiftDto ApproveShift(int id, int adminId, decimal countedCash, string? note);
        List<StaffOptionDto> GetStaffOptions();

        StaffHomeDto GetStaffHome(int staffId);
        ShiftReportDto? CloseStaffShift(int staffId);
        ShiftReportDto OpenStaffShift(int staffId);
        List<ShiftReportDto> ListStaffReports(int staffId);
        ShiftReportDto? GetStaffReport(int staffId, int shiftId);
        void AcceptHandover(int staffId);
        List<AdminBookingDto> LookupBookings(string? query);
        ExchangeBookingResult ExchangeBooking(int id, ExchangeBookingRequest request, int staffId);
    }
}