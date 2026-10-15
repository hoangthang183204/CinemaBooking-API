namespace CinemaBooking.Application.DTO
{
    public class HomeBlockDto
    {
        public int Id { get; set; }
        public string Kind { get; set; } = "";
        public string Title { get; set; } = "";
        public string? Subtitle { get; set; }
        public string? Tag { get; set; }
        public string? DateLabel { get; set; }
        public string? ImageUrl { get; set; }
        public string? AccentColor { get; set; }
        public int? MovieId { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }

    public class SaveHomeBlockRequest
    {
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

    public class HomeContentDto
    {
        public List<HomeBlockDto> Heroes { get; set; } = new();
        public List<HomeBlockDto> Snacks { get; set; } = new();
    }
}
