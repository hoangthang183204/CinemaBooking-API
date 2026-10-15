namespace CinemaBooking.Domain.Entities
{
    public class HomeBlock
    {
        public int Id { get; set; }
        public string Kind { get; set; } = "hero";
        public string Title { get; set; } = "";
        public string? Subtitle { get; set; }
        public string? Tag { get; set; }
        public string? DateLabel { get; set; }
        public string? ImageUrl { get; set; }
        public string? AccentColor { get; set; }
        public int? MovieId { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
