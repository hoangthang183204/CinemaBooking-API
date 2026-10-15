using System;
using System.Collections.Generic;

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
    }

    public class ConfirmPaymentResponse
    {
        public int BookingId { get; set; }
        public string Status { get; set; } = "";
        public DateTime? PaidAt { get; set; }
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
    }

    public class StaffBookingDetailDto
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
        public List<CinemaBooking.Application.DTO.BookingConcessionLineDto> Concessions { get; set; } = new();
        public List<ElectronicTicketQRDto> Tickets { get; set; } = new();
    }
}