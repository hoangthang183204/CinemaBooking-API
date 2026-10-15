using System;
using System.Collections.Generic;

namespace CinemaBooking.Application.DTO
{
    public class BookingDetailDto
    {
        public int Id { get; set; }
        public int ShowTimeId { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = ""; // Pending, Paid, Cancelled
        public DateTime CreatedAt { get; set; }
        public string MovieTitle { get; set; } = "";
        public string RoomName { get; set; } = "";
        public DateTime ShowTimeStart { get; set; }
        public List<string> SeatNumbers { get; set; } = new();
        public List<BookingConcessionLineDto> Concessions { get; set; } = new();
        public List<ElectronicTicketDto> Tickets { get; set; } = new();
    }

    public class BookingConcessionLineDto
    {
        public string Name { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class ConcessionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public string Category { get; set; } = "";
        public int SortOrder { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }
    }

    public class SaveConcessionRequest
    {
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public string Category { get; set; } = "Bắp";
        public int SortOrder { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateConcessionStockRequest
    {
        public int StockQuantity { get; set; }
    }
}
