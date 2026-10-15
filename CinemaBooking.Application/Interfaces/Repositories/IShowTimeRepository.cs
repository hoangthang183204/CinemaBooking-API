using CinemaBooking.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaBooking.Application.Interfaces.Repositories
{
    public interface IShowTimeRepository
    {
        ShowTime? GetById(int showTimeId);
        List<ShowTime> GetAll();
        void Add(ShowTime showTime);
    }
}
