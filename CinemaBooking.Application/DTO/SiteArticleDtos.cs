namespace CinemaBooking.Application.DTO
{
    public class SiteArticleDto
    {
        public int Id { get; set; }
        public string Page { get; set; } = "";
        public string Slot { get; set; } = "";
        public string Title { get; set; } = "";
        public string? Meta { get; set; }
        public string? Body { get; set; }
        public string? Extra { get; set; }
        public string? ImageUrl { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }

    public class SaveSiteArticleRequest
    {
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

    public class SitePageDto
    {
        public string Lead { get; set; } = "";
        public List<SiteArticleDto> Items { get; set; } = new();
        public List<SiteArticleDto> Notes { get; set; } = new();
    }

    public class SitePagesDto
    {
        public SitePageDto News { get; set; } = new();
        public SitePageDto Promotions { get; set; } = new();
        public SitePageDto Prices { get; set; } = new();
        public SitePageDto Festivals { get; set; } = new();
        public SitePageDto About { get; set; } = new();
    }
}
