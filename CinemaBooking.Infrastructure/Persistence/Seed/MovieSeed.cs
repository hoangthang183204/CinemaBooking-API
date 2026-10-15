using CinemaBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaBooking.Infrastructure.Persistence.Seed
{
    public class MovieSeed
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Movie>().HasData(
                new Movie { Id = 1, Title = "Avatar", Duration = 180, IsActive = true },
                new Movie { Id = 2, Title = "Inception", Duration = 148, IsActive = true }
            );
        }
    }
}
