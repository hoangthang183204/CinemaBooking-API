using System;

namespace CinemaBooking.Domain.Entities
{
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public int Duration { get; set; }
        public bool IsActive { get; set; }

        // Metadata mở rộng
        public string? PosterUrl { get; set; }
        public string? Genre { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string? Description { get; set; }
        public string? Director { get; set; }
        public string? Country { get; set; }
        public string? Language { get; set; }
        public string? Cast { get; set; }
        public string? TrailerUrl { get; set; }
        public decimal? Rating { get; set; }
    }
}