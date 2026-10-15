using CinemaBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaBooking.Infrastructure.Persistence.Seed
{
    public class SeatSeed
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Seat>().HasData(
                // Room 1
                new Seat { Id = 1, RoomId = 1, SeatNumber = "A1" },
                new Seat { Id = 2, RoomId = 1, SeatNumber = "A2" },
                new Seat { Id = 3, RoomId = 1, SeatNumber = "A3" },

                // Room 2
                new Seat { Id = 4, RoomId = 2, SeatNumber = "B1" },
                new Seat { Id = 5, RoomId = 2, SeatNumber = "B2" }
            );
        }
    }
}
