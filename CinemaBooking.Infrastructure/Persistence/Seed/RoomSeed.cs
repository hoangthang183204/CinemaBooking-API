using CinemaBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaBooking.Infrastructure.Persistence.Seed
{
    public class RoomSeed
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Room>().HasData(
                new Room { Id = 1, Name = "Room 1" },
                new Room { Id = 2, Name = "Room 2" }
            );
        }
    }
}
