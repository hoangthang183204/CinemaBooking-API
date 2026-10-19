using System;
using System.Collections.Generic;
using CinemaBooking.Application.DTO.Admin;

namespace CinemaBooking.Application.DTO.Staff
{
    // ========== TẠO BOOKING TẠI QUẦY ==========
    public class CreateCounterBookingRequest
    {
        public int ShowtimeId { get; set; }
        public List<int> SeatIds { get; set; } = new();
        public List<CinemaBooking.Application.DTO.ConcessionPickDto> Concessions { get; set; } = new();

        // Thông tin khách (optional)
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerEmail { get; set; }

        // Ghi chú
        public string? Note { get; set; }
    }

    public class CounterBookingResponse
    {
        public int BookingId { get; set; }
        public decimal TotalPrice { get; set; }
        public string MovieTitle { get; set; } = "";
        public string RoomName { get; set; } = "";
        public DateTime ShowTimeStart { get; set; }
        public List<string> SeatNumbers { get; set; } = new();
        public List<CinemaBooking.Application.DTO.BookingConcessionLineDto> Concessions { get; set; } = new();

        // QR thanh toán
        public string PaymentQRCode { get; set; } = "";
        public string PaymentQRImageUrl { get; set; } = "";
        public string PaymentUrl { get; set; } = "";
        public DateTime ExpiresAt { get; set; }

        public string Status { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }

    // ========== XÁC NHẬN THANH TOÁN ==========
    public class ConfirmPaymentRequest
    {
        public string? Note { get; set; }
        /// <summary>cash hoặc transfer.</summary>
        public string? Method { get; set; }
        public decimal? CashReceived { get; set; }
    }

    public class ConfirmPaymentResponse
    {
        public int BookingId { get; set; }
        public string Status { get; set; } = "";
        public DateTime? PaidAt { get; set; }
        public string Method { get; set; } = "";
        public decimal ChangeAmount { get; set; }
        public List<ElectronicTicketQRDto> Tickets { get; set; } = new();
    }

    public class ElectronicTicketQRDto
    {
        public int TicketId { get; set; }
        public string SeatNumber { get; set; } = "";
        public string QRCode { get; set; } = "";
        public string QRImageUrl { get; set; } = "";
    }

    // ========== CHECK-IN ==========
    public class CheckInQRRequest
    {
        public string QRCode { get; set; } = "";
    }

    public class CheckInResponse
    {
        public int BookingId { get; set; }
        public string MovieTitle { get; set; } = "";
        public string RoomName { get; set; } = "";
        public DateTime ShowTimeStart { get; set; }
        public string SeatNumber { get; set; } = "";
        public DateTime CheckedInAt { get; set; }
        /// <summary>ok, used, wrong, unpaid.</summary>
        public string Tone { get; set; } = "ok";
        public string Message { get; set; } = "";
    }

    public class StaffBookingDetailDto
    {
        public int Id { get; set; }
        public string UserEmail { get; set; } = "";
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public int MovieId { get; set; }
        public string MovieTitle { get; set; } = "";
        public string RoomName { get; set; } = "";
        public DateTime ShowTimeStart { get; set; }
        public List<string> SeatNumbers { get; set; } = new();
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public List<CinemaBooking.Application.DTO.BookingConcessionLineDto> Concessions { get; set; } = new();
        public List<ElectronicTicketQRDto> Tickets { get; set; } = new();
    }

    public class StaffHomeDto
    {
        public decimal CinemaRevenueToday { get; set; }
        public int BookingsToday { get; set; }
        public int PaidToday { get; set; }
        public int PendingBookings { get; set; }
        public bool ShiftOpen { get; set; }
        public DateTime? ShiftStartedAt { get; set; }
        public decimal MyCash { get; set; }
        public decimal MyTransfer { get; set; }
        public decimal MyRevenue { get; set; }
        public int MyTickets { get; set; }
        public int MyBookings { get; set; }
        public string? PendingFromName { get; set; }
        public decimal PendingCash { get; set; }
        public bool PendingOwnHandover { get; set; }
        public List<RecentBookingDto> RecentBookings { get; set; } = new();
    }

    public class ShiftReportDto
    {
        public int ShiftId { get; set; }
        public string StaffName { get; set; } = "";
        public DateTime StartedAt { get; set; }
        public DateTime EndedAt { get; set; }
        public decimal CashTotal { get; set; }
        public decimal TransferTotal { get; set; }
        public decimal Total { get; set; }
        public decimal CinemaRevenueToday { get; set; }
        public int TicketCount { get; set; }
        public int BookingCount { get; set; }
        public List<ShiftLineDto> Lines { get; set; } = new();
    }

    public class ShiftLineDto
    {
        public int BookingId { get; set; }
        public string MovieTitle { get; set; } = "";
        public string Method { get; set; } = "";
        public decimal Amount { get; set; }
        public int Seats { get; set; }
    }

    public class AdminShiftDto
    {
        public int Id { get; set; }
        public int StaffId { get; set; }
        public string StaffName { get; set; } = "";
        public string StaffEmail { get; set; } = "";
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        /// <summary>Open, Closed, Approved</summary>
        public string Status { get; set; } = "";
        public decimal ExpectedCash { get; set; }
        public decimal ExpectedTransfer { get; set; }
        public decimal? CountedCash { get; set; }
        public decimal? CashDifference { get; set; }
        public int TicketCount { get; set; }
        public int BookingCount { get; set; }
        public string? Note { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public List<ShiftLineDto> Lines { get; set; } = new();
    }

    public class AssignShiftRequest
    {
        public int StaffId { get; set; }
        public string? Note { get; set; }
    }

    public class ApproveShiftRequest
    {
        public decimal CountedCash { get; set; }
        public string? Note { get; set; }
    }

    public class StaffOptionDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
    }

    public class ExchangeBookingRequest
    {
        public int ShowtimeId { get; set; }
        public List<int> SeatIds { get; set; } = new();
    }

    public class ExchangeBookingResult
    {
        public int BookingId { get; set; }
        public decimal OldTotal { get; set; }
        public decimal NewTotal { get; set; }
        public decimal Difference { get; set; }
        public string Message { get; set; } = "";
        public string RoomName { get; set; } = "";
        public DateTime ShowTimeStart { get; set; }
        public List<string> SeatNumbers { get; set; } = new();
    }
}