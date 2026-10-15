using CinemaBooking.Application.Common;
using CinemaBooking.Application.DTO;
using CinemaBooking.Application.Interfaces;
using CinemaBooking.Domain.Entities;
using CinemaBooking.Infrastructure.Data;

namespace CinemaBooking.Infrastructure.Services
{
    public class SiteContentService : ISiteContentService
    {
        private static readonly string[] Pages = { "news", "promotion", "price", "festival", "about" };
        private readonly CinemaDbContext _context;

        public SiteContentService(CinemaDbContext context)
        {
            _context = context;
        }

        public SitePagesDto GetPublic()
        {
            var active = _context.SiteArticles
                .Where(a => a.IsActive)
                .OrderBy(a => a.SortOrder)
                .ThenBy(a => a.Id)
                .ToList();

            return new SitePagesDto
            {
                News = MapPage("news", active),
                Promotions = MapPage("promotion", active),
                Prices = MapPage("price", active),
                Festivals = MapPage("festival", active),
                About = MapPage("about", active)
            };
        }

        public List<SiteArticleDto> GetAll()
        {
            return _context.SiteArticles
                .OrderBy(a => a.Page)
                .ThenBy(a => a.SortOrder)
                .ThenBy(a => a.Id)
                .Select(a => Map(a))
                .ToList();
        }

        public SiteArticleDto? Create(SaveSiteArticleRequest request)
        {
            if (!IsValid(request)) return null;

            var article = new SiteArticle();
            Apply(article, request);
            _context.SiteArticles.Add(article);
            _context.SaveChanges();
            return Map(article);
        }

        public SiteArticleDto? Update(int id, SaveSiteArticleRequest request)
        {
            if (!IsValid(request)) return null;

            var article = _context.SiteArticles.FirstOrDefault(a => a.Id == id);
            if (article == null) return null;

            Apply(article, request);
            _context.SaveChanges();
            return Map(article);
        }

        public bool Delete(int id)
        {
            var article = _context.SiteArticles.FirstOrDefault(a => a.Id == id);
            if (article == null) return false;

            _context.SiteArticles.Remove(article);
            _context.SaveChanges();
            return true;
        }

        private static SitePageDto MapPage(string page, List<SiteArticle> all)
        {
            var rows = all.Where(a => a.Page == page).ToList();
            var intro = rows.FirstOrDefault(a => a.Slot == "intro");
            var itemSlot = page == "price" ? "row" : "card";

            return new SitePageDto
            {
                Lead = intro?.Body ?? "",
                Items = rows.Where(a => a.Slot == itemSlot).Select(Map).ToList(),
                Notes = page == "price"
                    ? rows.Where(a => a.Slot == "card").Select(Map).ToList()
                    : new List<SiteArticleDto>()
            };
        }

        private static bool IsValid(SaveSiteArticleRequest request)
        {
            if (!Pages.Contains(request.Page)) return false;
            if (request.Slot is not ("intro" or "card" or "row")) return false;
            if (request.Slot == "row" && request.Page != "price") return false;
            if (request.Slot == "intro") return !string.IsNullOrWhiteSpace(request.Body);
            return !string.IsNullOrWhiteSpace(request.Title);
        }

        private static void Apply(SiteArticle article, SaveSiteArticleRequest request)
        {
            article.Page = request.Page;
            article.Slot = request.Slot;
            article.Title = string.IsNullOrWhiteSpace(request.Title) ? "Mở đầu" : InputGuard.Plain(request.Title, 200);
            article.Meta = InputGuard.Optional(request.Meta, 200);
            article.Body = InputGuard.Optional(request.Body, 4000);
            article.Extra = InputGuard.Optional(request.Extra, 200);
            article.ImageUrl = InputGuard.MediaUrl(request.ImageUrl);
            article.SortOrder = request.SortOrder;
            article.IsActive = request.IsActive;
        }

        private static string? Clean(string? value)
        {
            var text = value?.Trim();
            return string.IsNullOrEmpty(text) ? null : text;
        }

        private static SiteArticleDto Map(SiteArticle article) => new()
        {
            Id = article.Id,
            Page = article.Page,
            Slot = article.Slot,
            Title = article.Title,
            Meta = article.Meta,
            Body = article.Body,
            Extra = article.Extra,
            ImageUrl = article.ImageUrl,
            SortOrder = article.SortOrder,
            IsActive = article.IsActive
        };
    }
}
