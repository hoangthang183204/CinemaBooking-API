namespace CinemaBooking.Domain.Entities
{
    public class SiteArticle
    {
        public int Id { get; set; }
        public string Page { get; set; } = "news";
        public string Slot { get; set; } = "card";
        public string Title { get; set; } = "";
        public string? Meta { get; set; }
        public string? Body { get; set; }
        public string? Extra { get; set; }
        public string? ImageUrl { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
