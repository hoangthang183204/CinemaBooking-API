using CinemaBooking.Application.Common;
using CinemaBooking.Application.DTO;
using CinemaBooking.Application.Interfaces;
using CinemaBooking.Domain.Entities;
using CinemaBooking.Infrastructure.Data;

namespace CinemaBooking.Infrastructure.Services
{
    public class HomeContentService : IHomeContentService
    {
        private readonly CinemaDbContext _context;

        public HomeContentService(CinemaDbContext context)
        {
            _context = context;
        }

        public HomeContentDto GetPublic()
        {
            var active = _context.HomeBlocks
                .Where(b => b.IsActive)
                .OrderBy(b => b.SortOrder)
                .ThenBy(b => b.Id)
                .ToList();

            return new HomeContentDto
            {
                Heroes = active.Where(b => b.Kind == "hero").Select(Map).ToList(),
                Snacks = active.Where(b => b.Kind == "snack").Select(Map).ToList()
            };
        }

        public List<HomeBlockDto> GetAll()
        {
            return _context.HomeBlocks
                .OrderBy(b => b.Kind)
                .ThenBy(b => b.SortOrder)
                .ThenBy(b => b.Id)
                .Select(b => Map(b))
                .ToList();
        }

        public HomeBlockDto? Create(SaveHomeBlockRequest request)
        {
            if (!IsValid(request)) return null;

            var block = new HomeBlock();
            Apply(block, request);
            _context.HomeBlocks.Add(block);
            _context.SaveChanges();
            return Map(block);
        }

        public HomeBlockDto? Update(int id, SaveHomeBlockRequest request)
        {
            if (!IsValid(request)) return null;

            var block = _context.HomeBlocks.FirstOrDefault(b => b.Id == id);
            if (block == null) return null;

            Apply(block, request);
            _context.SaveChanges();
            return Map(block);
        }

        public bool Delete(int id)
        {
            var block = _context.HomeBlocks.FirstOrDefault(b => b.Id == id);
            if (block == null) return false;

            _context.HomeBlocks.Remove(block);
            _context.SaveChanges();
            return true;
        }

        private static bool IsValid(SaveHomeBlockRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Title)) return false;
            return request.Kind is "hero" or "snack";
        }

        private static void Apply(HomeBlock block, SaveHomeBlockRequest request)
        {
            block.Kind = request.Kind;
            block.Title = InputGuard.Plain(request.Title, 200);
            block.Subtitle = InputGuard.Optional(request.Subtitle, 300);
            block.Tag = InputGuard.Optional(request.Tag, 80);
            block.DateLabel = InputGuard.Optional(request.DateLabel, 40);
            block.ImageUrl = InputGuard.MediaUrl(request.ImageUrl);
            block.AccentColor = InputGuard.HexColor(request.AccentColor) ?? "#e63946";
            block.MovieId = request.MovieId is > 0 ? request.MovieId : null;
            block.SortOrder = request.SortOrder;
            block.IsActive = request.IsActive;
        }

        private static string? Clean(string? value)
        {
            var text = value?.Trim();
            return string.IsNullOrEmpty(text) ? null : text;
        }

        private static HomeBlockDto Map(HomeBlock block) => new()
        {
            Id = block.Id,
            Kind = block.Kind,
            Title = block.Title,
            Subtitle = block.Subtitle,
            Tag = block.Tag,
            DateLabel = block.DateLabel,
            ImageUrl = block.ImageUrl,
            AccentColor = block.AccentColor,
            MovieId = block.MovieId,
            SortOrder = block.SortOrder,
            IsActive = block.IsActive
        };
    }
}
