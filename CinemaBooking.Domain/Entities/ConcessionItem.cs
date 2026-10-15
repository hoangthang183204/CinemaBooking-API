namespace CinemaBooking.Domain.Entities
{
    public class ConcessionItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public string Category { get; set; } = "";
        public int SortOrder { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
