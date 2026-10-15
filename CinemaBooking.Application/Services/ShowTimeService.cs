using CinemaBooking.Application.Common;
using CinemaBooking.Application.DTO;
using CinemaBooking.Application.Interfaces;
using CinemaBooking.Application.Interfaces.Repositories;
using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CinemaBooking.Application.Services
{
    public class ShowTimeService
    {
        private readonly IShowTimeRepository _showTimeRepo;
        private readonly ISeatRepository _seatRepo;
        private readonly ITicketRepository _ticketRepo;
        private readonly IMovieRepository _movieRepo;
        private readonly IRoomRepository _roomRepo;
        private readonly ISeatHoldService _seatHolds;

        public ShowTimeService(
            IShowTimeRepository showTimeRepo,
            ISeatRepository seatRepo,
            ITicketRepository ticketRepo,
            IMovieRepository movieRepo,
            IRoomRepository roomRepo,
            ISeatHoldService seatHolds)
        {
            _showTimeRepo = showTimeRepo;
            _seatRepo = seatRepo;
            _ticketRepo = ticketRepo;
            _movieRepo = movieRepo;
            _roomRepo = roomRepo;
            _seatHolds = seatHolds;
        }

        public List<ShowTimeDto> GetShowTimes()
        {
            var now = DateTime.UtcNow;
            var list = _showTimeRepo.GetAll()
                .Where(s => !ShowTimeClock.HasStarted(s.StartTime, now));
            return list.Select(s => new ShowTimeDto
            {
                Id = s.Id,
                StartTime = s.StartTime,
                MovieId = s.MovieId,
                MovieTitle = s.Movie?.Title ?? "",
                RoomId = s.RoomId,
                RoomName = s.Room?.Name ?? ""
            }).ToList();
        }

        public RoomSeatDto? GetRoomSeatsByShowTime(int showTimeId, int? userId = null)
        {
            var showTime = _showTimeRepo.GetById(showTimeId);
            if (showTime == null) return null;
            if (ShowTimeClock.HasStarted(showTime.StartTime))
                throw new InvalidOperationException("Suất chiếu đã qua giờ, không thể đặt vé.");

            var seats = _seatRepo.GetByRoomId(showTime.RoomId);
            var bookedSeatIds = _ticketRepo.GetBookedSeatIds(showTimeId, TicketStatus.Booked);
            var availability = _seatHolds.GetAvailability(showTimeId, userId);

            return new RoomSeatDto
            {
                RoomId = showTime.Room.Id,
                RoomName = showTime.Room.Name,
                Seats = seats.Select(s => new SeatStatusDto
                {
                    SeatId = s.Id,
                    SeatNumber = s.SeatNumber,
                    SeatType = (int)s.SeatType,
                    IsBooked = bookedSeatIds.Contains(s.Id) || availability.OpenBookingSeatIds.Contains(s.Id),
                    IsHeld = availability.HeldByOthers.Contains(s.Id),
                    IsHeldByMe = availability.HeldByUser.Contains(s.Id)
                }).ToList()
            };
        }

        /// <summary>Tạo suất chiếu mới. Trả về null nếu Movie hoặc Room không tồn tại.</summary>
        public ShowTimeDto? CreateShowTime(int movieId, int roomId, DateTime startTime)
        {
            var movie = _movieRepo.GetById(movieId);
            var room = _roomRepo.GetById(roomId);
            if (movie == null || room == null)
                return null;

            var showTime = new ShowTime
            {
                MovieId = movieId,
                RoomId = roomId,
                StartTime = startTime
            };
            _showTimeRepo.Add(showTime);

            return new ShowTimeDto
            {
                Id = showTime.Id,
                StartTime = showTime.StartTime,
                MovieId = movieId,
                MovieTitle = movie.Title,
                RoomId = roomId,
                RoomName = room.Name
            };
        }
    }
}
