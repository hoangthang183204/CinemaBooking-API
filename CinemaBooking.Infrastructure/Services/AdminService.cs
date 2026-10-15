using CinemaBooking.Application.Common;
using CinemaBooking.Application.DTO;
using CinemaBooking.Application.DTO.Admin;
using CinemaBooking.Application.DTO.Staff;
using CinemaBooking.Application.Interfaces;
using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Enums;
using CinemaBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CinemaBooking.Infrastructure.Services
{
    public class AdminService : IAdminService
    {
        private readonly CinemaDbContext _context;
        private readonly IConfiguration _config;

        public AdminService(CinemaDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // ========== DASHBOARD ==========
        public DashboardStatsDto GetDashboardStats()
        {
            var paidBookings = _context.Bookings.Where(b => b.Status == BookingStatus.Paid);
            var allBookings = _context.Bookings.AsQueryable();

            var recentRows = allBookings
                .OrderByDescending(b => b.CreatedAt)
                .Take(5)
                .Select(b => new
                {
                    b.Id,
                    b.UserId,
                    MovieTitle = b.ShowTime.Movie.Title,
                    b.TotalPrice,
                    b.Status,
                    b.CreatedAt
                })
                .ToList();
            var recentUserIds = recentRows.Where(b => b.UserId != null).Select(b => b.UserId!.Value).Distinct().ToList();
            var recentEmails = _context.Users
                .Where(u => recentUserIds.Contains(u.Id))
                .ToDictionary(u => u.Id, u => u.Email);
            var recent = recentRows.Select(b => new RecentBookingDto
            {
                Id = b.Id,
                MovieTitle = b.MovieTitle,
                UserEmail = b.UserId != null && recentEmails.TryGetValue(b.UserId.Value, out var email) ? email : "Guest",
                TotalPrice = b.TotalPrice,
                Status = b.Status.ToString(),
                CreatedAt = b.CreatedAt
            }).ToList();

            var topMovies = allBookings
                .Include(b => b.ShowTime).ThenInclude(s => s.Movie)
                .GroupBy(b => new { b.ShowTime.MovieId, b.ShowTime.Movie.Title })
                .Select(g => new TopMovieDto
                {
                    MovieId = g.Key.MovieId,
                    Title = g.Key.Title,
                    BookingCount = g.Count(),
                    Revenue = g.Where(b => b.Status == BookingStatus.Paid).Sum(b => b.TotalPrice)
                })
                .OrderByDescending(x => x.BookingCount)
                .Take(5)
                .ToList();

            return new DashboardStatsDto
            {
                TotalRevenue = paidBookings.Any() ? paidBookings.Sum(b => b.TotalPrice) : 0,
                TotalBookings = allBookings.Count(),
                TotalUsers = _context.Users.Count(),
                TotalMovies = _context.Movies.Count(),
                TotalRooms = _context.Rooms.Count(),
                TotalShowTimes = _context.ShowTimes.Count(),
                PaidBookings = paidBookings.Count(),
                PendingBookings = allBookings.Count(b => b.Status == BookingStatus.Pending),
                RecentBookings = recent,
                TopMovies = topMovies
            };
        }

        // ========== MOVIES ==========
        public List<AdminMovieDto> GetAllMovies()
        {
            return _context.Movies
                .OrderByDescending(m => m.Id)
                .Select(MapMovie)
                .ToList();
        }

        public AdminMovieDto? GetMovieById(int id)
        {
            var m = _context.Movies.FirstOrDefault(x => x.Id == id);
            return m == null ? null : MapMovie(m);
        }

        public AdminMovieDto CreateMovie(CreateUpdateMovieRequest req)
        {
            var m = new Movie
            {
                Title = InputGuard.Plain(req.Title, 200),
                Duration = req.Duration,
                IsActive = req.IsActive,
                PosterUrl = InputGuard.MediaUrl(req.PosterUrl),
                Genre = InputGuard.Optional(req.Genre, 200),
                ReleaseDate = req.ReleaseDate,
                Description = InputGuard.Optional(req.Description, 4000),
                Director = InputGuard.Optional(req.Director, 200),
                Country = InputGuard.Optional(req.Country, 80),
                Language = InputGuard.Optional(req.Language, 120),
                Cast = InputGuard.Optional(req.Cast, 500),
                TrailerUrl = InputGuard.MediaUrl(req.TrailerUrl),
                Rating = req.Rating
            };
            _context.Movies.Add(m);
            _context.SaveChanges();
            return MapMovie(m);
        }

        public AdminMovieDto? UpdateMovie(int id, CreateUpdateMovieRequest req)
        {
            var m = _context.Movies.FirstOrDefault(x => x.Id == id);
            if (m == null) return null;

            m.Title = InputGuard.Plain(req.Title, 200);
            m.Duration = req.Duration;
            m.IsActive = req.IsActive;
            m.PosterUrl = InputGuard.MediaUrl(req.PosterUrl);
            m.Genre = InputGuard.Optional(req.Genre, 200);
            m.ReleaseDate = req.ReleaseDate;
            m.Description = InputGuard.Optional(req.Description, 4000);
            m.Director = InputGuard.Optional(req.Director, 200);
            m.Country = InputGuard.Optional(req.Country, 80);
            m.Language = InputGuard.Optional(req.Language, 120);
            m.Cast = InputGuard.Optional(req.Cast, 500);
            m.TrailerUrl = InputGuard.MediaUrl(req.TrailerUrl);
            m.Rating = req.Rating;
            _context.SaveChanges();
            return MapMovie(m);
        }

        public bool DeleteMovie(int id)
        {
            var m = _context.Movies.FirstOrDefault(x => x.Id == id);
            if (m == null) return false;

            // Kiểm tra có suất chiếu nào không
            var hasShowTimes = _context.ShowTimes.Any(s => s.MovieId == id);
            if (hasShowTimes)
            {
                // Không xóa cứng — chuyển thành inactive
                m.IsActive = false;
                _context.SaveChanges();
                return true;
            }

            _context.Movies.Remove(m);
            _context.SaveChanges();
            return true;
        }

        // ========== ROOMS ==========
        public List<AdminRoomDto> GetAllRooms()
        {
            var seatCounts = _context.Seats
                .GroupBy(s => s.RoomId)
                .Select(g => new { RoomId = g.Key, Count = g.Count() })
                .ToDictionary(x => x.RoomId, x => x.Count);
            var showCounts = _context.ShowTimes
                .GroupBy(s => s.RoomId)
                .Select(g => new { RoomId = g.Key, Count = g.Count() })
                .ToDictionary(x => x.RoomId, x => x.Count);

            return _context.Rooms
                .OrderBy(r => r.Id)
                .Select(r => new { r.Id, r.Name })
                .AsEnumerable()
                .Select(r => new AdminRoomDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    SeatCount = seatCounts.TryGetValue(r.Id, out var seats) ? seats : 0,
                    ShowTimeCount = showCounts.TryGetValue(r.Id, out var shows) ? shows : 0
                })
                .ToList();
        }

        public AdminRoomDto? GetRoomById(int id)
        {
            var room = _context.Rooms.FirstOrDefault(x => x.Id == id);
            if (room == null) return null;
            return new AdminRoomDto
            {
                Id = room.Id,
                Name = room.Name,
                SeatCount = _context.Seats.Count(s => s.RoomId == id),
                ShowTimeCount = _context.ShowTimes.Count(s => s.RoomId == id)
            };
        }

        public AdminRoomDto CreateRoom(CreateUpdateRoomRequest req)
        {
            var room = new Room { Name = req.Name.Trim() };
            _context.Rooms.Add(room);
            _context.SaveChanges();

            // Tạo ghế theo layout
            GenerateSeats(room.Id, req.Rows, req.SeatsPerRow, req.CoupleRows);
            _context.SaveChanges();

            return GetRoomById(room.Id)!;
        }

        public AdminRoomDto? UpdateRoom(int id, CreateUpdateRoomRequest req)
        {
            var room = _context.Rooms.FirstOrDefault(r => r.Id == id);
            if (room == null) return null;

            room.Name = req.Name.Trim();

            // Nếu có ghế rồi thì không regenerate để tránh mất data
            var hasSeats = _context.Seats.Any(s => s.RoomId == id);
            if (!hasSeats)
            {
                GenerateSeats(room.Id, req.Rows, req.SeatsPerRow, req.CoupleRows);
            }

            _context.SaveChanges();
            return GetRoomById(id)!;
        }

        public bool DeleteRoom(int id)
        {
            var room = _context.Rooms.FirstOrDefault(r => r.Id == id);
            if (room == null) return false;

            var hasShowTimes = _context.ShowTimes.Any(s => s.RoomId == id);
            if (hasShowTimes) return false;   // Không cho xóa nếu có suất chiếu

            // Xóa ghế
            var seats = _context.Seats.Where(s => s.RoomId == id).ToList();
            _context.Seats.RemoveRange(seats);
            _context.Rooms.Remove(room);
            _context.SaveChanges();
            return true;
        }

        private void GenerateSeats(int roomId, int rows, int seatsPerRow, int coupleRows)
        {
            var seats = new List<Seat>();
            for (int r = 0; r < rows; r++)
            {
                var rowChar = (char)('A' + r);
                var isCoupleRow = r >= rows - coupleRows;   // hàng cuối là couple

                for (int c = 1; c <= seatsPerRow; c++)
                {
                    seats.Add(new Seat
                    {
                        RoomId = roomId,
                        SeatNumber = $"{rowChar}{c}",
                        SeatType = isCoupleRow ? SeatType.Couple : SeatType.Normal
                    });
                }
            }
            _context.Seats.AddRange(seats);
        }

        // ========== SHOWTIMES ==========
        public List<AdminShowTimeDto> GetAllShowTimes()
        {
            var booked = _context.Tickets
                .GroupBy(t => t.ShowTimeId)
                .Select(g => new { ShowTimeId = g.Key, Count = g.Count() })
                .ToDictionary(x => x.ShowTimeId, x => x.Count);
            var seats = _context.Seats
                .GroupBy(s => s.RoomId)
                .Select(g => new { RoomId = g.Key, Count = g.Count() })
                .ToDictionary(x => x.RoomId, x => x.Count);

            return _context.ShowTimes
                .OrderByDescending(s => s.StartTime)
                .Select(s => new
                {
                    s.Id,
                    s.MovieId,
                    MovieTitle = s.Movie.Title,
                    s.RoomId,
                    RoomName = s.Room.Name,
                    s.StartTime
                })
                .AsEnumerable()
                .Select(s => new AdminShowTimeDto
                {
                    Id = s.Id,
                    MovieId = s.MovieId,
                    MovieTitle = s.MovieTitle,
                    RoomId = s.RoomId,
                    RoomName = s.RoomName,
                    StartTime = s.StartTime,
                    BookedSeats = booked.TryGetValue(s.Id, out var bookedCount) ? bookedCount : 0,
                    TotalSeats = seats.TryGetValue(s.RoomId, out var seatCount) ? seatCount : 0
                })
                .ToList();
        }

        public AdminShowTimeDto? GetShowTimeById(int id)
        {
            var row = _context.ShowTimes
                .Where(s => s.Id == id)
                .Select(s => new
                {
                    s.Id,
                    s.MovieId,
                    MovieTitle = s.Movie.Title,
                    s.RoomId,
                    RoomName = s.Room.Name,
                    s.StartTime
                })
                .FirstOrDefault();
            if (row == null) return null;

            return new AdminShowTimeDto
            {
                Id = row.Id,
                MovieId = row.MovieId,
                MovieTitle = row.MovieTitle,
                RoomId = row.RoomId,
                RoomName = row.RoomName,
                StartTime = row.StartTime,
                BookedSeats = _context.Tickets.Count(t => t.ShowTimeId == id),
                TotalSeats = _context.Seats.Count(s => s.RoomId == row.RoomId)
            };
        }

        public AdminShowTimeDto? CreateShowTime(CreateUpdateShowTimeRequest req)
        {
            var movie = _context.Movies.FirstOrDefault(m => m.Id == req.MovieId);
            var room = _context.Rooms.FirstOrDefault(r => r.Id == req.RoomId);
            if (movie == null || room == null) return null;

            var st = new ShowTime
            {
                MovieId = req.MovieId,
                RoomId = req.RoomId,
                StartTime = req.StartTime
            };
            _context.ShowTimes.Add(st);
            _context.SaveChanges();
            return GetShowTimeById(st.Id);
        }

        public AdminShowTimeDto? UpdateShowTime(int id, CreateUpdateShowTimeRequest req)
        {
            var st = _context.ShowTimes.FirstOrDefault(s => s.Id == id);
            if (st == null) return null;

            var movie = _context.Movies.FirstOrDefault(m => m.Id == req.MovieId);
            var room = _context.Rooms.FirstOrDefault(r => r.Id == req.RoomId);
            if (movie == null || room == null) return null;

            st.MovieId = req.MovieId;
            st.RoomId = req.RoomId;
            st.StartTime = req.StartTime;
            _context.SaveChanges();
            return GetShowTimeById(id);
        }

        public bool DeleteShowTime(int id)
        {
            var st = _context.ShowTimes.FirstOrDefault(s => s.Id == id);
            if (st == null) return false;

            var hasBookings = _context.Bookings.Any(b => b.ShowTimeId == id);
            if (hasBookings) return false;   // Không cho xóa nếu có booking

            _context.ShowTimes.Remove(st);
            _context.SaveChanges();
            return true;
        }

        // ========== USERS ==========
        public List<AdminUserDto> GetAllUsers()
        {
            var bookingCounts = _context.Bookings
                .Where(b => b.UserId != null)
                .GroupBy(b => b.UserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToList()
                .Where(x => x.UserId != null)
                .ToDictionary(x => x.UserId!.Value, x => x.Count);

            return _context.Users
                .OrderByDescending(u => u.Id)
                .Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.FullName,
                    u.Phone,
                    u.Role,
                    u.IsActive,
                    u.CreatedAt
                })
                .AsEnumerable()
                .Select(u => new AdminUserDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    FullName = u.FullName,
                    Phone = u.Phone,
                    Role = u.Role.ToString(),
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    BookingCount = bookingCounts.TryGetValue(u.Id, out var count) ? count : 0
                })
                .ToList();
        }

        public AdminUserDto? GetUserById(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null) return null;
            return new AdminUserDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Phone = user.Phone,
                Role = user.Role.ToString(),
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                BookingCount = _context.Bookings.Count(b => b.UserId == id)
            };
        }

        public AdminUserDto? UpdateUserRole(int id, string role)
        {
            var u = _context.Users.FirstOrDefault(x => x.Id == id);
            if (u == null) return null;

            if (Enum.TryParse<UserRole>(role, true, out var newRole))
                u.Role = newRole;

            _context.SaveChanges();
            return GetUserById(id);
        }

        public AdminUserDto? UpdateUserStatus(int id, bool isActive)
        {
            var u = _context.Users.FirstOrDefault(x => x.Id == id);
            if (u == null) return null;

            u.IsActive = isActive;
            _context.SaveChanges();
            return GetUserById(id);
        }

        // ========== BOOKINGS ==========
        public List<AdminBookingDto> GetAllBookings()
        {
            var bookings = _context.Bookings
                .AsNoTracking()
                .Include(b => b.ShowTime).ThenInclude(s => s.Movie)
                .Include(b => b.ShowTime).ThenInclude(s => s.Room)
                .Include(b => b.BookingSeats).ThenInclude(bs => bs.Seat)
                .OrderByDescending(b => b.CreatedAt)
                .ToList();
            var userIds = bookings.Where(b => b.UserId != null).Select(b => b.UserId!.Value).Distinct().ToList();
            var emails = _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionary(u => u.Id, u => u.Email);

            return bookings.Select(b => new AdminBookingDto
            {
                Id = b.Id,
                UserEmail = b.UserId != null && emails.TryGetValue(b.UserId.Value, out var email) ? email : "Guest",
                MovieTitle = b.ShowTime.Movie.Title,
                RoomName = b.ShowTime.Room.Name,
                ShowTimeStart = b.ShowTime.StartTime,
                SeatNumbers = b.BookingSeats.Select(bs => bs.Seat.SeatNumber).OrderBy(s => s).ToList(),
                TotalPrice = b.TotalPrice,
                Status = b.Status.ToString(),
                CreatedAt = b.CreatedAt
            }).ToList();
        }

        public AdminBookingDto? GetBookingById(int id)
        {
            var booking = _context.Bookings
                .AsNoTracking()
                .Include(b => b.ShowTime).ThenInclude(s => s.Movie)
                .Include(b => b.ShowTime).ThenInclude(s => s.Room)
                .Include(b => b.BookingSeats).ThenInclude(bs => bs.Seat)
                .FirstOrDefault(b => b.Id == id);
            if (booking == null) return null;

            var email = booking.UserId == null
                ? "Guest"
                : _context.Users.Where(u => u.Id == booking.UserId).Select(u => u.Email).FirstOrDefault() ?? "Guest";

            return new AdminBookingDto
            {
                Id = booking.Id,
                UserEmail = email,
                MovieTitle = booking.ShowTime.Movie.Title,
                RoomName = booking.ShowTime.Room.Name,
                ShowTimeStart = booking.ShowTime.StartTime,
                SeatNumbers = booking.BookingSeats.Select(bs => bs.Seat.SeatNumber).OrderBy(s => s).ToList(),
                TotalPrice = booking.TotalPrice,
                Status = booking.Status.ToString(),
                CreatedAt = booking.CreatedAt
            };
        }

        public StaffBookingDetailDto? GetBookingDetail(int id)
        {
            var booking = _context.Bookings
                .Include(b => b.ShowTime).ThenInclude(s => s.Movie)
                .Include(b => b.ShowTime).ThenInclude(s => s.Room)
                .Include(b => b.BookingSeats).ThenInclude(bs => bs.Seat)
                .Include(b => b.Concessions)
                .Include(b => b.ElectronicTickets).ThenInclude(t => t.Seat)
                .FirstOrDefault(b => b.Id == id);

            if (booking == null) return null;

            var email = booking.UserId == null
                ? "Guest"
                : _context.Users.Where(u => u.Id == booking.UserId).Select(u => u.Email).FirstOrDefault() ?? "Guest";

            return new StaffBookingDetailDto
            {
                Id = booking.Id,
                UserEmail = email,
                MovieTitle = booking.ShowTime.Movie.Title,
                RoomName = booking.ShowTime.Room.Name,
                ShowTimeStart = booking.ShowTime.StartTime,
                SeatNumbers = booking.BookingSeats
                    .Select(bs => bs.Seat.SeatNumber)
                    .OrderBy(s => s)
                    .ToList(),
                TotalPrice = booking.TotalPrice,
                Status = booking.Status.ToString(),
                CreatedAt = booking.CreatedAt,
                Concessions = booking.Concessions.Select(c => new BookingConcessionLineDto
                {
                    Name = c.ItemName,
                    Quantity = c.Quantity,
                    UnitPrice = c.UnitPrice
                }).ToList(),
                Tickets = booking.ElectronicTickets.Select(t =>
                {
                    var code = t.QRCode ?? "";
                    return new ElectronicTicketQRDto
                    {
                        TicketId = t.Id,
                        SeatNumber = t.Seat.SeatNumber,
                        QRCode = code,
                        QRImageUrl = "https://api.qrserver.com/v1/create-qr-code/?size=180x180&data="
                            + Uri.EscapeDataString(code)
                    };
                }).ToList()
            };
        }

        public bool CancelBooking(int id)
        {
            var b = _context.Bookings.FirstOrDefault(x => x.Id == id);
            if (b == null) return false;
            if (b.Status == BookingStatus.Cancelled || b.Status == BookingStatus.Refunded || b.Status == BookingStatus.RefundCompleted) return true;
            var wasPaid = b.Status == BookingStatus.Paid;

            var lines = _context.BookingConcessions.Where(c => c.BookingId == id).ToList();
            var itemIds = lines.Select(c => c.ConcessionItemId).Distinct().ToList();
            var stockItems = _context.ConcessionItems.Where(c => itemIds.Contains(c.Id)).ToList();
            foreach (var line in lines)
            {
                var item = stockItems.FirstOrDefault(c => c.Id == line.ConcessionItemId);
                if (item != null) item.StockQuantity += line.Quantity;
            }

            b.Status = wasPaid ? BookingStatus.Refunded : BookingStatus.Cancelled;

            // Xóa tickets để nhả ghế và hủy QR vào phòng
            var seatIds = _context.BookingSeats.Where(bs => bs.BookingId == id).Select(bs => bs.SeatId).ToList();
            var toRemove = _context.Tickets
                .Where(t => t.ShowTimeId == b.ShowTimeId && seatIds.Contains(t.SeatId))
                .ToList();
            _context.Tickets.RemoveRange(toRemove);
            var electronic = _context.ElectronicTickets.Where(t => t.BookingId == id).ToList();
            _context.ElectronicTickets.RemoveRange(electronic);

            _context.SaveChanges();
            return true;
        }

        public (bool Ok, string Message) CancelForCustomer(int id, int userId)
        {
            var booking = _context.Bookings
                .Include(b => b.ShowTime)
                .FirstOrDefault(b => b.Id == id);
            if (booking == null) return (false, "Không tìm thấy đơn");
            if (booking.UserId != userId) return (false, "Bạn không hủy được đơn của người khác");
            if (booking.Status == BookingStatus.Cancelled)
                return (true, "Đơn đã được hủy");
            if (booking.Status == BookingStatus.Refunded)
                return (true, "Vé đã hủy. Mang mã đơn ra quầy để nhận lại tiền.");
            if (booking.Status == BookingStatus.RefundCompleted)
                return (true, "Tiền đã được hoàn tại quầy.");
            if (booking.Status != BookingStatus.Pending && booking.Status != BookingStatus.Paid)
                return (false, "Đơn này không hủy được");
            if (!ShowTimeClock.CanCustomerCancel(booking.ShowTime.StartTime))
                return (false, "Chỉ hủy được trước giờ chiếu ít nhất 2 giờ.");

            var paid = booking.Status == BookingStatus.Paid;
            if (!CancelBooking(id)) return (false, "Không hủy được đơn");

            return paid
                ? (true, "Đã hủy vé. Ghế được trả lại. Mang mã đơn ra quầy để nhận lại tiền.")
                : (true, "Đã hủy đơn. Ghế và bắp nước được trả lại.");
        }

        public bool TryConfirmBankTransfer(string? content, decimal amount)
        {
            if (string.IsNullOrWhiteSpace(content) || amount <= 0) return false;
            var match = System.Text.RegularExpressions.Regex.Match(content, @"VE\s*(\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (!match.Success || !int.TryParse(match.Groups[1].Value, out var bookingId)) return false;

            var booking = _context.Bookings.FirstOrDefault(b => b.Id == bookingId);
            if (booking == null) return false;
            if (booking.Status == BookingStatus.Paid) return true;
            if (booking.Status != BookingStatus.Pending) return false;
            if (booking.TotalPrice != amount) return false;

            return ConfirmBookingPayment(bookingId, 0, "Chuyen khoan dung noi dung VE " + bookingId) != null;
        }

        public bool CompleteRefund(int id)
        {
            var booking = _context.Bookings.FirstOrDefault(x => x.Id == id);
            if (booking == null) return false;
            if (booking.Status == BookingStatus.RefundCompleted) return true;
            if (booking.Status != BookingStatus.Refunded) return false;

            booking.Status = BookingStatus.RefundCompleted;
            _context.SaveChanges();
            return true;
        }

        public bool MarkBookingAsPaid(int id)
        {
            var b = _context.Bookings.FirstOrDefault(x => x.Id == id);
            if (b == null) return false;

            b.Status = BookingStatus.Paid;

            // Tạo ElectronicTickets nếu chưa có
            var existing = _context.ElectronicTickets.Any(et => et.BookingId == id);
            if (!existing)
            {
                var seats = _context.BookingSeats.Where(bs => bs.BookingId == id).ToList();
                foreach (var bs in seats)
                {
                    _context.ElectronicTickets.Add(new ElectronicTicket
                    {
                        BookingId = id,
                        SeatId = bs.SeatId,
                        QRCode = $"BK{id}-S{bs.SeatId}-{Guid.NewGuid():N}"
                    });
                }
            }

            _context.SaveChanges();
            return true;
        }

        // ========== HELPERS ==========
        private static AdminMovieDto MapMovie(Movie m) => new()
        {
            Id = m.Id,
            Title = m.Title,
            Duration = m.Duration,
            IsActive = m.IsActive,
            PosterUrl = m.PosterUrl,
            Genre = m.Genre,
            ReleaseDate = m.ReleaseDate,
            Description = m.Description,
            Director = m.Director,
            Country = m.Country,
            Language = m.Language,
            Cast = m.Cast,
            TrailerUrl = m.TrailerUrl,
            Rating = m.Rating
        };

        public CounterBookingResponse? CreateCounterBooking(
    CreateCounterBookingRequest request,
    int staffId)
        {
            var showtimeId = request.ShowtimeId;
            var seatIds = request.SeatIds?.Distinct().ToList() ?? new List<int>();

            if (seatIds.Count == 0 || seatIds.Count > SeatHoldService.MaxSeatsPerHold) return null;

            var showTime = _context.ShowTimes
                .Include(s => s.Movie)
                .Include(s => s.Room)
                .FirstOrDefault(s => s.Id == showtimeId);

            if (showTime == null) return null;
            if (ShowTimeClock.HasStarted(showTime.StartTime))
                throw new InvalidOperationException("Suất chiếu đã qua giờ, không thể đặt vé.");

            // Validate ghế thuộc đúng phòng
            var roomSeatIds = _context.Seats
                .Where(s => s.RoomId == showTime.RoomId)
                .Select(s => s.Id)
                .ToHashSet();

            if (seatIds.Any(id => !roomSeatIds.Contains(id))) return null;

            // Check ghế đã đặt chưa
            var bookedSeatIds = _context.Tickets
                .Where(t => t.ShowTimeId == showtimeId)
                .Select(t => t.SeatId)
                .ToHashSet();

            if (seatIds.Any(id => bookedSeatIds.Contains(id))) return null;

            var now = DateTime.UtcNow;
            var holds = _context.SeatHolds
                .Where(h => h.ShowTimeId == showtimeId && seatIds.Contains(h.SeatId) && h.ExpiresAt > now)
                .ToList();
            if (holds.Count > 0) return null;

            const decimal PricePerSeat = 75000m;

            var picks = (request.Concessions ?? new List<ConcessionPickDto>())
                .Where(c => c.Quantity > 0)
                .GroupBy(c => c.ConcessionId)
                .Select(g => new ConcessionPickDto { ConcessionId = g.Key, Quantity = g.Sum(x => x.Quantity) })
                .ToList();
            if (picks.Any(c => c.Quantity > 8))
                throw new InvalidOperationException("Tối đa 8 phần cho mỗi món bắp nước.");

            var itemIds = picks.Select(c => c.ConcessionId).ToList();
            var menu = itemIds.Count == 0
                ? new List<ConcessionItem>()
                : _context.ConcessionItems.Where(c => itemIds.Contains(c.Id) && c.IsActive).ToList();
            if (menu.Count != itemIds.Count)
                throw new InvalidOperationException("Có món bắp nước không còn bán.");

            var concessionLines = new List<BookingConcession>();
            decimal snackTotal = 0;
            foreach (var pick in picks)
            {
                var item = menu.First(c => c.Id == pick.ConcessionId);
                if (item.StockQuantity < pick.Quantity)
                    throw new InvalidOperationException($"Không đủ {item.Name}. Còn {item.StockQuantity}.");
                item.StockQuantity -= pick.Quantity;
                snackTotal += item.Price * pick.Quantity;
                concessionLines.Add(new BookingConcession
                {
                    ConcessionItemId = item.Id,
                    ItemName = item.Name,
                    Quantity = pick.Quantity,
                    UnitPrice = item.Price
                });
            }

            var totalPrice = PricePerSeat * seatIds.Count + snackTotal;

            // Tạo booking — UserId = null (khách vãng lai)
            var booking = new Booking
            {
                UserId = null,   // Khách vãng lai, không có account
                ShowTimeId = showtimeId,
                TotalPrice = totalPrice,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var seatId in seatIds)
            {
                booking.BookingSeats.Add(new BookingSeat
                {
                    SeatId = seatId,
                    Price = PricePerSeat
                });
            }

            foreach (var line in concessionLines)
                booking.Concessions.Add(line);

            _context.Bookings.Add(booking);
            _context.SaveChanges();

            // Tạo Payment record
            var payment = new Payment
            {
                BookingId = booking.Id,
                Amount = totalPrice,
                Method = PaymentMethod.VNPay,   // Default
                Status = PaymentStatus.Pending,
                GatewayOrderId = $"COUNTER-{booking.Id}-{DateTime.UtcNow:yyyyMMddHHmmss}",
                CreatedAt = DateTime.UtcNow
            };
            _context.Payments.Add(payment);
            _context.SaveChanges();

            var qrData = VietQr.Payload(
                _config["VietQR:BankBin"] ?? "970407",
                _config["VietQR:AccountNumber"] ?? "19076145844018",
                _config["VietQR:AccountName"] ?? "TECHCOMBANK",
                totalPrice,
                "VE " + booking.Id);
            var qrImageUrl = VietQr.ImageUrl(qrData, 300);

            // Payment URL — nếu khách muốn thanh toán online
            var paymentUrl = $"https://localhost:5173/payment/{booking.Id}";

            // Lấy seat numbers
            var seatNumbers = _context.Seats
                .Where(s => seatIds.Contains(s.Id))
                .Select(s => s.SeatNumber)
                .ToList();

            return new CounterBookingResponse
            {
                BookingId = booking.Id,
                TotalPrice = totalPrice,
                MovieTitle = showTime.Movie.Title,
                RoomName = showTime.Room.Name,
                ShowTimeStart = showTime.StartTime,
                SeatNumbers = seatNumbers,
                Concessions = concessionLines.Select(c => new BookingConcessionLineDto
                {
                    Name = c.ItemName,
                    Quantity = c.Quantity,
                    UnitPrice = c.UnitPrice
                }).ToList(),
                PaymentQRCode = qrData,
                PaymentQRImageUrl = qrImageUrl,
                PaymentUrl = paymentUrl,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                Status = booking.Status.ToString(),
                CreatedAt = booking.CreatedAt
            };
        }

        // ========== XÁC NHẬN THANH TOÁN ==========
        public ConfirmPaymentResponse? ConfirmBookingPayment(
            int bookingId,
            int staffId,
            string? note)
        {
            var booking = _context.Bookings
                .Include(b => b.BookingSeats).ThenInclude(bs => bs.Seat)
                .FirstOrDefault(b => b.Id == bookingId);

            if (booking == null) return null;
            if (booking.Status == BookingStatus.Paid) return null;

            // Update status
            booking.Status = BookingStatus.Paid;

            // Update Payment
            var payment = _context.Payments
                .OrderByDescending(p => p.Id)
                .FirstOrDefault(p => p.BookingId == bookingId);

            if (payment != null)
            {
                payment.Status = PaymentStatus.Success;
                payment.PaidAt = DateTime.UtcNow;
                payment.GatewayResponse = $"Confirmed by staff #{staffId}" +
                    (string.IsNullOrEmpty(note) ? "" : $" | Note: {note}");
            }

            // Tạo Tickets (để chặn ghế)
            var seatIds = booking.BookingSeats.Select(bs => bs.SeatId).ToList();
            var takenSeatIds = _context.Tickets
                .Where(t => t.ShowTimeId == booking.ShowTimeId && seatIds.Contains(t.SeatId))
                .Select(t => t.SeatId)
                .ToHashSet();

            foreach (var bs in booking.BookingSeats)
            {
                if (takenSeatIds.Contains(bs.SeatId)) continue;
                _context.Tickets.Add(new Ticket
                {
                    ShowTimeId = booking.ShowTimeId,
                    SeatId = bs.SeatId,
                    Price = bs.Price
                });
            }

            // Tạo ElectronicTickets với QR check-in
            var existingTickets = _context.ElectronicTickets
                .Where(et => et.BookingId == bookingId)
                .ToList();

            var tickets = new List<ElectronicTicketQRDto>();

            foreach (var bs in booking.BookingSeats)
            {
                var existing = existingTickets.FirstOrDefault(et => et.SeatId == bs.SeatId);
                if (existing != null)
                {
                    tickets.Add(new ElectronicTicketQRDto
                    {
                        TicketId = existing.Id,
                        SeatNumber = bs.Seat?.SeatNumber ?? "",
                        QRCode = existing.QRCode,
                        QRImageUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=200x200&data={Uri.EscapeDataString(existing.QRCode)}"
                    });
                    continue;
                }

                var qrCode = $"BK{bookingId}-S{bs.SeatId}-{Guid.NewGuid():N}";
                var et = new ElectronicTicket
                {
                    BookingId = bookingId,
                    SeatId = bs.SeatId,
                    QRCode = qrCode
                };
                _context.ElectronicTickets.Add(et);
                _context.SaveChanges();

                tickets.Add(new ElectronicTicketQRDto
                {
                    TicketId = et.Id,
                    SeatNumber = bs.Seat?.SeatNumber ?? "",
                    QRCode = qrCode,
                    QRImageUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=200x200&data={Uri.EscapeDataString(qrCode)}"
                });
            }

            _context.SaveChanges();

            return new ConfirmPaymentResponse
            {
                BookingId = bookingId,
                Status = booking.Status.ToString(),
                PaidAt = payment?.PaidAt ?? DateTime.UtcNow,
                Tickets = tickets
            };
        }

        // ========== CHECK-IN QR ==========
        public CheckInResponse? CheckInByQRCode(string qrCode)
        {
            if (string.IsNullOrWhiteSpace(qrCode)) return null;

            var ticket = _context.ElectronicTickets
                .Include(et => et.Booking).ThenInclude(b => b.ShowTime).ThenInclude(s => s.Movie)
                .Include(et => et.Booking).ThenInclude(b => b.ShowTime).ThenInclude(s => s.Room)
                .Include(et => et.Seat)
                .FirstOrDefault(et => et.QRCode == qrCode);

            if (ticket == null) return null;

            // Check đã thanh toán
            if (ticket.Booking.Status != BookingStatus.Paid)
                throw new InvalidOperationException("Vé chưa được thanh toán");

            // TODO: Nếu muốn chống dùng lại, thêm field IsCheckedIn vào ElectronicTicket
            // Hiện tại chỉ trả về thông tin

            return new CheckInResponse
            {
                BookingId = ticket.BookingId,
                MovieTitle = ticket.Booking.ShowTime.Movie.Title,
                RoomName = ticket.Booking.ShowTime.Room.Name,
                ShowTimeStart = ticket.Booking.ShowTime.StartTime,
                SeatNumber = ticket.Seat?.SeatNumber ?? "",
                CheckedInAt = DateTime.UtcNow
            };
        }
    }
}