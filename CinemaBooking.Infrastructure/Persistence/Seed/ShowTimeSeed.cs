using CinemaBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Infrastructure.Persistence.Seed
{
    public class ShowTimeSeed
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ShowTime>().HasData(
                new ShowTime { Id = 1, StartTime = DateTime.Today.AddDays(1).AddHours(19), MovieId = 1, RoomId = 1 },
                new ShowTime { Id = 2, StartTime = DateTime.Today.AddDays(1).AddHours(21), MovieId = 2, RoomId = 1 },
                new ShowTime { Id = 3, StartTime = DateTime.Today.AddDays(2).AddHours(20), MovieId = 1, RoomId = 2 }
            );
        }
    }
}
