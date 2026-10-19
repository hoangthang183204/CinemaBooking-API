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
        private static readonly System.Text.Json.JsonSerializerOptions ReportJsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

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

            var occupancy = BuildOccupancy();
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
                RefundPending = allBookings.Count(b => b.Status == BookingStatus.Refunded),
                RecentBookings = recent,
                TopMovies = topMovies,
                RevenueByDay = BuildRevenueByDay(),
                OccupancyRate = occupancy.Count == 0 ? 0 : Math.Round(occupancy.Average(x => x.Rate), 1),
                RoomOccupancy = occupancy,
                Alerts = BuildAlerts()
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

        private string? RoomTurnaround(int roomId, int movieId, DateTime start, int? ignoreId)
        {
            const int gapMinutes = 30;
            var duration = _context.Movies.Where(m => m.Id == movieId).Select(m => m.Duration).FirstOrDefault();
            if (duration <= 0) duration = 1;
            var end = start.AddMinutes(duration);
            var others = _context.ShowTimes
                .Where(s => s.RoomId == roomId && (ignoreId == null || s.Id != ignoreId.Value))
                .Select(s => new { s.StartTime, Title = s.Movie.Title, s.Movie.Duration, Room = s.Room.Name })
                .ToList();
            foreach (var other in others)
            {
                var otherMinutes = other.Duration > 0 ? other.Duration : 1;
                var otherEnd = other.StartTime.AddMinutes(otherMinutes);
                if (start >= otherEnd.AddMinutes(gapMinutes) || other.StartTime >= end.AddMinutes(gapMinutes))
                    continue;
                var nextOpen = otherEnd.AddMinutes(gapMinutes);
                return $"Phòng {other.Room} đang chiếu \"{other.Title}\" đến {otherEnd:HH:mm dd/MM}. Suất tiếp theo phải từ {nextOpen:HH:mm dd/MM}, cách 30 phút sau khi phim kết thúc.";
            }
            return null;
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
                .Where(s => !s.IsBlocked)
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
                TotalSeats = _context.Seats.Count(s => s.RoomId == row.RoomId && !s.IsBlocked)
            };
        }

        public AdminShowTimeDto? CreateShowTime(CreateUpdateShowTimeRequest req)
        {
            var movie = _context.Movies.FirstOrDefault(m => m.Id == req.MovieId);
            var room = _context.Rooms.FirstOrDefault(r => r.Id == req.RoomId);
            if (movie == null || room == null) return null;
            var hours = CinemaPricing.OutsideOpenHours(_context, req.StartTime);
            if (hours != null) throw new InvalidOperationException(hours);
            var clash = RoomTurnaround(req.RoomId, req.MovieId, req.StartTime, null);
            if (clash != null) throw new InvalidOperationException(clash);

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
            var hours = CinemaPricing.OutsideOpenHours(_context, req.StartTime);
            if (hours != null) throw new InvalidOperationException(hours);
            var clash = RoomTurnaround(req.RoomId, req.MovieId, req.StartTime, id);
            if (clash != null) throw new InvalidOperationException(clash);

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

        private static readonly string[] StaffPositions = { "Admin", "ShiftManager", "TicketSeller", "TicketChecker" };

        public List<StaffProfileDto> GetStaffDirectory()
        {
            var people = StaffPeople();
            var stats = ShiftStats();
            var duties = DutyStates();
            return people.Select(user => MapProfile(user, stats, duties, false)).OrderBy(s => s.FullName).ToList();
        }

        public StaffProfileDto? GetStaffProfile(int id)
        {
            var user = StaffPeople().FirstOrDefault(u => u.Id == id);
            if (user == null) return null;
            return MapProfile(user, ShiftStats(), DutyStates(), true);
        }

        public StaffProfileDto CreateStaff(SaveStaffRequest request)
        {
            var position = NormalizePosition(request.Position);
            var email = (request.Email ?? "").Trim().ToLowerInvariant();
            var name = InputGuard.Plain(request.FullName, 128);
            if (name.Length < 2) throw new InvalidOperationException("Nhập họ tên nhân viên.");
            if (!email.Contains('@') || email.Length > 256) throw new InvalidOperationException("Email không hợp lệ.");
            if (_context.Users.Any(u => u.Email == email)) throw new InvalidOperationException("Email đã được dùng.");
            var password = request.Password ?? "";
            if (password.Length < 6) throw new InvalidOperationException("Mật khẩu tối thiểu 6 ký tự.");

            var user = new User
            {
                Email = email,
                FullName = name,
                Phone = CleanPhone(request.Phone),
                CitizenId = CleanCitizen(request.CitizenId),
                AvatarUrl = InputGuard.MediaUrl(request.AvatarUrl),
                Position = position,
                Role = position == "Admin" ? UserRole.Admin : UserRole.Staff,
                IsActive = request.IsActive,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            _context.SaveChanges();
            return MapProfile(user, ShiftStats(), DutyStates(), true);
        }

        public StaffProfileDto? UpdateStaff(int id, SaveStaffRequest request)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id && (u.Role == UserRole.Staff || u.Role == UserRole.Admin));
            if (user == null) return null;
            var position = NormalizePosition(request.Position);
            var email = (request.Email ?? "").Trim().ToLowerInvariant();
            var name = InputGuard.Plain(request.FullName, 128);
            if (name.Length < 2) throw new InvalidOperationException("Nhập họ tên nhân viên.");
            if (!email.Contains('@')) throw new InvalidOperationException("Email không hợp lệ.");
            if (_context.Users.Any(u => u.Email == email && u.Id != id)) throw new InvalidOperationException("Email đã được dùng.");
            if (!string.IsNullOrWhiteSpace(request.Password) && request.Password.Length < 6)
                throw new InvalidOperationException("Mật khẩu tối thiểu 6 ký tự.");

            user.FullName = name;
            user.Email = email;
            user.Phone = CleanPhone(request.Phone);
            user.CitizenId = CleanCitizen(request.CitizenId);
            user.AvatarUrl = InputGuard.MediaUrl(request.AvatarUrl);
            user.Position = position;
            user.Role = position == "Admin" ? UserRole.Admin : UserRole.Staff;
            user.IsActive = request.IsActive;
            if (!string.IsNullOrWhiteSpace(request.Password))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            _context.SaveChanges();
            return MapProfile(user, ShiftStats(), DutyStates(), true);
        }

        private List<User> StaffPeople()
        {
            return _context.Users.Where(u => u.Role == UserRole.Staff || u.Role == UserRole.Admin).ToList();
        }

        private Dictionary<int, (int Count, decimal Revenue)> ShiftStats()
        {
            return _context.StaffShifts
                .GroupBy(s => s.StaffId)
                .Select(g => new
                {
                    g.Key,
                    Count = g.Count(),
                    Revenue = g.Sum(s => (s.ExpectedCash ?? 0) + (s.ExpectedTransfer ?? 0))
                })
                .ToList()
                .ToDictionary(x => x.Key, x => (x.Count, x.Revenue));
        }

        private Dictionary<int, (bool Open, DateTime At)> DutyStates()
        {
            var latest = _context.StaffShifts
                .Select(s => new { s.Id, s.StaffId, s.StartedAt, s.EndedAt, s.AwaitingHandover, s.ReceivedAt })
                .ToList()
                .GroupBy(s => s.StaffId)
                .Select(g => g.OrderByDescending(s => s.Id).First());
            var map = new Dictionary<int, (bool Open, DateTime At)>();
            foreach (var row in latest)
            {
                if (row.EndedAt == null)
                    map[row.StaffId] = (true, row.StartedAt);
                else if (row.AwaitingHandover && row.ReceivedAt == null)
                    map[row.StaffId] = (false, row.EndedAt.Value);
            }
            return map;
        }

        private StaffProfileDto MapProfile(User user, Dictionary<int, (int Count, decimal Revenue)> stats, Dictionary<int, (bool Open, DateTime At)> duties, bool withShifts)
        {
            stats.TryGetValue(user.Id, out var row);
            var known = duties.TryGetValue(user.Id, out var duty);
            var dto = new StaffProfileDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                CitizenId = user.CitizenId,
                AvatarUrl = user.AvatarUrl,
                Position = string.IsNullOrWhiteSpace(user.Position)
                    ? (user.Role == UserRole.Admin ? "Admin" : "TicketSeller")
                    : user.Position,
                Role = user.Role.ToString(),
                IsActive = user.IsActive,
                ShiftCount = row.Count,
                Revenue = row.Revenue,
                OnDuty = known && duty.Open,
                HandoverPending = known && !duty.Open,
                DutyStartedAt = known ? UtcStamp(duty.At) : null
            };
            if (withShifts)
            {
                var shifts = _context.StaffShifts.Where(s => s.StaffId == user.Id).OrderByDescending(s => s.StartedAt).ToList();
                var people = new Dictionary<int, User> { [user.Id] = user };
                dto.Shifts = shifts.Select(s => MapShift(s, people, false)).ToList();
            }
            return dto;
        }

        private static string NormalizePosition(string? position)
        {
            var value = (position ?? "").Trim();
            if (!StaffPositions.Contains(value))
                throw new InvalidOperationException("Chọn chức vụ: Admin, Quản lý ca, Nhân viên bán vé hoặc Nhân viên soát vé.");
            return value;
        }

        private static string? CleanPhone(string? phone)
        {
            var text = new string((phone ?? "").Where(char.IsDigit).ToArray());
            if (text.Length == 0) return null;
            if (text.Length < 9 || text.Length > 11) throw new InvalidOperationException("Số điện thoại không hợp lệ.");
            return text;
        }

        private static string? CleanCitizen(string? citizenId)
        {
            var text = new string((citizenId ?? "").Where(char.IsDigit).ToArray());
            if (text.Length == 0) return null;
            if (text.Length != 9 && text.Length != 12) throw new InvalidOperationException("CCCD phải có 12 số.");
            return text;
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
            var people = _context.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.Email, u.Phone, u.FullName })
                .ToList()
                .ToDictionary(u => u.Id);

            return bookings.Select(b => new AdminBookingDto
            {
                Id = b.Id,
                UserEmail = !string.IsNullOrWhiteSpace(b.CustomerEmail)
                    ? b.CustomerEmail!
                    : b.UserId != null && people.TryGetValue(b.UserId.Value, out var person) ? person.Email : "Khách tại quầy",
                CustomerName = !string.IsNullOrWhiteSpace(b.CustomerName)
                    ? b.CustomerName
                    : b.UserId != null && people.TryGetValue(b.UserId.Value, out var named) ? named.FullName : null,
                CustomerPhone = !string.IsNullOrWhiteSpace(b.CustomerPhone)
                    ? b.CustomerPhone
                    : b.UserId != null && people.TryGetValue(b.UserId.Value, out var phoned) ? phoned.Phone : null,
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

            var user = booking.UserId == null
                ? null
                : _context.Users.Where(u => u.Id == booking.UserId)
                    .Select(u => new { u.Email, u.Phone, u.FullName })
                    .FirstOrDefault();

            return new StaffBookingDetailDto
            {
                Id = booking.Id,
                UserEmail = booking.CustomerEmail ?? user?.Email ?? "Khách tại quầy",
                CustomerName = booking.CustomerName ?? user?.FullName,
                CustomerPhone = booking.CustomerPhone ?? user?.Phone,
                MovieId = booking.ShowTime.MovieId,
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

            if (!wasPaid && !string.IsNullOrWhiteSpace(b.PromoCode))
            {
                var promo = _context.Promotions.FirstOrDefault(p => p.Code == b.PromoCode);
                if (promo != null && promo.UsedCount > 0) promo.UsedCount -= 1;
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

        public bool MarkBookingAsPaid(int id, int staffId = 0)
        {
            var note = staffId > 0 ? "Nhân viên xác nhận đã thanh toán" : "Quản trị xác nhận đã thanh toán";
            var method = staffId > 0 ? "cash" : "transfer";
            if (ConfirmBookingPayment(id, staffId, note, method) != null)
                return true;

            var b = _context.Bookings.FirstOrDefault(x => x.Id == id);
            if (b == null || b.Status != BookingStatus.Paid) return false;
            EnsureSuccessPayment(b, staffId);
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
            if (_context.Seats.Any(s => seatIds.Contains(s.Id) && s.IsBlocked)) return null;

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

            var quote = CinemaPricing.Quote(_context, showtimeId, seatIds.Count, snackTotal, null, false);
            var seatPrice = quote.SeatPrice;
            var totalPrice = quote.Total;

            // Tạo booking — UserId = null (khách vãng lai)
            var booking = new Booking
            {
                UserId = null,   // Khách vãng lai, không có account
                ShowTimeId = showtimeId,
                TotalPrice = totalPrice,
                DiscountAmount = quote.Discount,
                SurchargeAmount = quote.Surcharge,
                PromoCode = quote.Code,
                CustomerName = string.IsNullOrWhiteSpace(request.CustomerName) ? null : InputGuard.Plain(request.CustomerName, 120),
                CustomerPhone = string.IsNullOrWhiteSpace(request.CustomerPhone) ? null : InputGuard.Plain(request.CustomerPhone, 30),
                CustomerEmail = string.IsNullOrWhiteSpace(request.CustomerEmail) ? null : InputGuard.Plain(request.CustomerEmail, 160),
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var seatId in seatIds)
            {
                booking.BookingSeats.Add(new BookingSeat
                {
                    SeatId = seatId,
                    Price = seatPrice
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
            string? note,
            string? method = null,
            decimal? cashReceived = null)
        {
            var booking = _context.Bookings
                .Include(b => b.BookingSeats).ThenInclude(bs => bs.Seat)
                .FirstOrDefault(b => b.Id == bookingId);

            if (booking == null) return null;
            if (booking.Status == BookingStatus.Paid) return null;

            var payMethod = ParsePayMethod(method, staffId);
            if (payMethod == PaymentMethod.Cash && cashReceived != null && cashReceived.Value < booking.TotalPrice)
                throw new InvalidOperationException("Số tiền khách đưa chưa đủ để thanh toán.");

            // Update status
            booking.Status = BookingStatus.Paid;

            // Update Payment
            var payment = _context.Payments
                .OrderByDescending(p => p.Id)
                .FirstOrDefault(p => p.BookingId == bookingId);

            if (payment == null)
            {
                payment = new Payment
                {
                    BookingId = bookingId,
                    Amount = booking.TotalPrice,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Payments.Add(payment);
            }

            payment.Method = payMethod;
            payment.Status = PaymentStatus.Success;
            payment.PaidAt = DateTime.UtcNow;
            payment.Amount = booking.TotalPrice;
            if (staffId > 0) payment.StaffId = staffId;
            payment.GatewayResponse = $"Confirmed by staff #{staffId}" +
                (string.IsNullOrEmpty(note) ? "" : $" | Note: {note}");

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

            var change = payMethod == PaymentMethod.Cash && cashReceived != null
                ? cashReceived.Value - booking.TotalPrice
                : 0m;

            return new ConfirmPaymentResponse
            {
                BookingId = bookingId,
                Status = booking.Status.ToString(),
                PaidAt = payment.PaidAt ?? DateTime.UtcNow,
                Method = payMethod.ToString(),
                ChangeAmount = change < 0 ? 0 : change,
                Tickets = tickets
            };
        }

        // ========== CHECK-IN QR ==========
        public CheckInResponse? CheckInByQRCode(string qrCode)
        {
            if (string.IsNullOrWhiteSpace(qrCode)) return null;
            var code = qrCode.Trim();

            var ticket = _context.ElectronicTickets
                .Include(et => et.Booking).ThenInclude(b => b.ShowTime).ThenInclude(s => s.Movie)
                .Include(et => et.Booking).ThenInclude(b => b.ShowTime).ThenInclude(s => s.Room)
                .Include(et => et.Seat)
                .FirstOrDefault(et => et.QRCode == code);

            if (ticket == null && int.TryParse(code, out var bookingId))
            {
                ticket = _context.ElectronicTickets
                    .Include(et => et.Booking).ThenInclude(b => b.ShowTime).ThenInclude(s => s.Movie)
                    .Include(et => et.Booking).ThenInclude(b => b.ShowTime).ThenInclude(s => s.Room)
                    .Include(et => et.Seat)
                    .Where(et => et.BookingId == bookingId)
                    .OrderBy(et => et.CheckedInAt.HasValue)
                    .ThenBy(et => et.Id)
                    .FirstOrDefault();
            }

            if (ticket == null) return null;
            return AdmitTicket(ticket);
        }

        public StaffHomeDto GetStaffHome(int staffId)
        {
            var shift = _context.StaffShifts.FirstOrDefault(s => s.StaffId == staffId && s.EndedAt == null);
            var totals = TodayCollected();
            var (startUtc, endUtc) = VietnamTodayUtc();
            var cinema = CinemaRevenueToday();
            var waiting = _context.StaffShifts
                .Where(s => s.AwaitingHandover && s.ReceivedAt == null && s.EndedAt != null)
                .OrderByDescending(s => s.EndedAt)
                .FirstOrDefault();
            string? pendingFrom = null;
            decimal pendingCash = 0;
            var pendingOwn = false;
            if (waiting != null && shift == null)
            {
                pendingCash = waiting.HandoverCash ?? waiting.ExpectedCash ?? 0;
                if (waiting.StaffId == staffId)
                    pendingOwn = true;
                else
                    pendingFrom = _context.Users.Where(u => u.Id == waiting.StaffId).Select(u => u.FullName).FirstOrDefault() ?? "nhân viên";
            }

            var recentRows = _context.Bookings
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

            return new StaffHomeDto
            {
                CinemaRevenueToday = cinema,
                BookingsToday = _context.Bookings.Count(b => b.CreatedAt >= startUtc && b.CreatedAt < endUtc),
                PaidToday = _context.Bookings.Count(b => b.Status == BookingStatus.Paid && b.CreatedAt >= startUtc && b.CreatedAt < endUtc),
                PendingBookings = _context.Bookings.Count(b => b.Status == BookingStatus.Pending),
                ShiftOpen = shift != null,
                ShiftStartedAt = shift == null ? null : UtcStamp(shift.StartedAt),
                MyCash = totals.Cash,
                MyTransfer = totals.Transfer,
                MyRevenue = totals.Cash + totals.Transfer,
                MyTickets = totals.Tickets,
                MyBookings = totals.Bookings,
                PendingFromName = pendingFrom,
                PendingCash = pendingCash,
                PendingOwnHandover = pendingOwn,
                RecentBookings = recentRows.Select(b => new RecentBookingDto
                {
                    Id = b.Id,
                    MovieTitle = b.MovieTitle,
                    UserEmail = b.UserId != null && recentEmails.TryGetValue(b.UserId.Value, out var email) ? email : "Khách tại quầy",
                    TotalPrice = b.TotalPrice,
                    Status = b.Status.ToString(),
                    CreatedAt = b.CreatedAt
                }).ToList()
            };
        }

        public ShiftReportDto OpenStaffShift(int staffId)
        {
            if (_context.StaffShifts.Any(s => s.AwaitingHandover && s.ReceivedAt == null && s.EndedAt != null))
                throw new InvalidOperationException("Ca trước đang chờ bàn giao. Nhân viên kế tiếp cần đăng nhập và xác nhận nhận ca.");
            var shift = EnsureOpenShift(staffId);
            return BuildShiftReport(shift, staffId, DateTime.UtcNow);
        }

        public void AcceptHandover(int staffId)
        {
            if (_context.StaffShifts.Any(s => s.StaffId == staffId && s.EndedAt == null))
                throw new InvalidOperationException("Bạn đang có ca mở.");
            var waiting = _context.StaffShifts
                .Where(s => s.AwaitingHandover && s.ReceivedAt == null && s.EndedAt != null)
                .OrderByDescending(s => s.EndedAt)
                .FirstOrDefault();
            if (waiting == null)
                throw new InvalidOperationException("Không có ca nào đang chờ bàn giao.");
            if (waiting.StaffId == staffId)
                throw new InvalidOperationException("Người vừa bàn giao không tự nhận ca. Hãy để nhân viên kế tiếp đăng nhập.");

            var cash = waiting.HandoverCash ?? waiting.ExpectedCash ?? 0;
            waiting.AwaitingHandover = false;
            waiting.ReceivedAt = DateTime.UtcNow;
            waiting.ReceivedById = staffId;
            var older = _context.StaffShifts
                .Where(s => s.StaffId == waiting.StaffId && s.Id < waiting.Id && s.AwaitingHandover)
                .ToList();
            foreach (var shift in older)
                shift.AwaitingHandover = false;
            _context.StaffShifts.Add(new StaffShift
            {
                StaffId = staffId,
                StartedAt = DateTime.UtcNow,
                OpeningCash = cash,
                Note = "Nhận ca"
            });
            _context.SaveChanges();
        }

        public ShiftReportDto? CloseStaffShift(int staffId)
        {
            var shift = _context.StaffShifts.FirstOrDefault(s => s.StaffId == staffId && s.EndedAt == null);
            if (shift == null) return null;
            var ended = DateTime.UtcNow;
            var report = BuildShiftReport(shift, staffId, ended);
            shift.EndedAt = ended;
            shift.ExpectedCash = report.CashTotal;
            shift.ExpectedTransfer = report.TransferTotal;
            shift.HandoverCash = report.CashTotal;
            shift.AwaitingHandover = true;
            shift.ReportJson = System.Text.Json.JsonSerializer.Serialize(report);
            _context.SaveChanges();
            return report;
        }

        public List<ShiftReportDto> ListStaffReports(int staffId)
        {
            var shifts = _context.StaffShifts
                .Where(s => s.StaffId == staffId && s.EndedAt != null)
                .OrderByDescending(s => s.EndedAt)
                .Take(200)
                .ToList();
            var list = new List<ShiftReportDto>();
            var changed = false;
            foreach (var shift in shifts)
            {
                var report = ReadOrBuildReport(shift, staffId, ref changed);
                report.Lines = new List<ShiftLineDto>();
                list.Add(report);
            }
            if (changed) _context.SaveChanges();
            return list;
        }

        public ShiftReportDto? GetStaffReport(int staffId, int shiftId)
        {
            var shift = _context.StaffShifts.FirstOrDefault(s => s.Id == shiftId && s.StaffId == staffId && s.EndedAt != null);
            if (shift == null) return null;
            var changed = false;
            var report = ReadOrBuildReport(shift, staffId, ref changed);
            if (changed) _context.SaveChanges();
            return report;
        }

        public List<StaffOptionDto> GetStaffOptions()
        {
            return _context.Users
                .Where(u => u.IsActive && u.Role == UserRole.Staff)
                .OrderBy(u => u.FullName)
                .Select(u => new StaffOptionDto { Id = u.Id, FullName = u.FullName, Email = u.Email })
                .ToList();
        }

        public List<AdminShiftDto> GetShifts()
        {
            var shifts = _context.StaffShifts.OrderByDescending(s => s.StartedAt).ToList();
            var ids = shifts.Select(s => s.StaffId).Distinct().ToList();
            var people = _context.Users.Where(u => ids.Contains(u.Id)).ToDictionary(u => u.Id);
            return shifts.Select(s => MapShift(s, people, false)).ToList();
        }

        public AdminShiftDto? GetShift(int id)
        {
            var shift = _context.StaffShifts.FirstOrDefault(s => s.Id == id);
            if (shift == null) return null;
            var people = _context.Users.Where(u => u.Id == shift.StaffId).ToDictionary(u => u.Id);
            return MapShift(shift, people, true);
        }

        public AdminShiftDto AssignShift(int staffId, int adminId, string? note)
        {
            var staff = _context.Users.FirstOrDefault(u => u.Id == staffId && u.IsActive && u.Role == UserRole.Staff);
            if (staff == null)
                throw new InvalidOperationException("Chỉ gán ca cho tài khoản nhân viên đang hoạt động.");
            if (_context.StaffShifts.Any(s => s.StaffId == staffId && s.EndedAt == null))
                throw new InvalidOperationException($"{staff.FullName} đang có ca mở.");

            var shift = new StaffShift
            {
                StaffId = staffId,
                StartedAt = DateTime.UtcNow,
                AssignedById = adminId,
                Note = string.IsNullOrWhiteSpace(note) ? null : InputGuard.Optional(note, 400)
            };
            _context.StaffShifts.Add(shift);
            _context.SaveChanges();
            var people = new Dictionary<int, User> { [staff.Id] = staff };
            return MapShift(shift, people, false);
        }

        public AdminShiftDto ApproveShift(int id, int adminId, decimal countedCash, string? note)
        {
            var shift = _context.StaffShifts.FirstOrDefault(s => s.Id == id);
            if (shift == null)
                throw new InvalidOperationException("Không tìm thấy ca.");
            if (shift.EndedAt == null)
                throw new InvalidOperationException("Ca đang mở. Nhân viên cần kết ca trước khi duyệt.");
            if (shift.ApprovedAt != null)
                throw new InvalidOperationException("Ca này đã được duyệt.");
            if (countedCash < 0)
                throw new InvalidOperationException("Tiền mặt đếm được không âm.");

            if (shift.ExpectedCash == null)
            {
                var totals = ShiftTotals(shift);
                shift.ExpectedCash = totals.Cash;
                shift.ExpectedTransfer = totals.Transfer;
            }

            shift.CountedCash = countedCash;
            shift.ApprovedAt = DateTime.UtcNow;
            shift.ApprovedById = adminId;
            if (!string.IsNullOrWhiteSpace(note))
                shift.Note = InputGuard.Optional(note, 400);
            _context.SaveChanges();

            var people = _context.Users.Where(u => u.Id == shift.StaffId).ToDictionary(u => u.Id);
            return MapShift(shift, people, true);
        }

        public List<AdminBookingDto> LookupBookings(string? query)
        {
            var q = (query ?? "").Trim();
            if (q.Length == 0) return GetAllBookings();

            var all = GetAllBookings();
            var needle = q.ToLowerInvariant();
            return all.Where(b =>
                b.Id.ToString() == q
                || (b.UserEmail ?? "").ToLowerInvariant().Contains(needle)
                || (b.CustomerPhone ?? "").Contains(q)
                || (b.CustomerName ?? "").ToLowerInvariant().Contains(needle)
                || (b.MovieTitle ?? "").ToLowerInvariant().Contains(needle)
            ).ToList();
        }

        public ExchangeBookingResult ExchangeBooking(int id, ExchangeBookingRequest request, int staffId)
        {
            var seatIds = request.SeatIds?.Distinct().ToList() ?? new List<int>();
            if (seatIds.Count == 0)
                throw new InvalidOperationException("Chọn ghế mới cho khách.");

            var booking = _context.Bookings
                .Include(b => b.BookingSeats)
                .Include(b => b.Concessions)
                .Include(b => b.ElectronicTickets)
                .Include(b => b.ShowTime)
                .FirstOrDefault(b => b.Id == id);
            if (booking == null)
                throw new InvalidOperationException("Không tìm thấy đơn.");
            if (booking.Status is BookingStatus.Cancelled or BookingStatus.Refunded or BookingStatus.RefundCompleted)
                throw new InvalidOperationException("Đơn đã hủy hoặc đang hoàn tiền, không đổi được.");

            var show = _context.ShowTimes
                .Include(s => s.Movie)
                .Include(s => s.Room)
                .FirstOrDefault(s => s.Id == request.ShowtimeId);
            if (show == null)
                throw new InvalidOperationException("Suất chiếu không tồn tại.");
            if (ShowTimeClock.HasStarted(show.StartTime))
                throw new InvalidOperationException("Suất chiếu đã qua giờ, không đổi sang suất này.");

            var roomSeatIds = _context.Seats.Where(s => s.RoomId == show.RoomId).Select(s => s.Id).ToHashSet();
            if (seatIds.Any(seatId => !roomSeatIds.Contains(seatId)))
                throw new InvalidOperationException("Ghế không thuộc phòng của suất mới.");
            if (_context.Seats.Any(s => seatIds.Contains(s.Id) && s.IsBlocked))
                throw new InvalidOperationException("Có ghế đang khóa, không bán được.");

            var taken = _context.Tickets
                .Where(t => t.ShowTimeId == show.Id && seatIds.Contains(t.SeatId))
                .Select(t => t.SeatId)
                .ToHashSet();
            var oldSeatIds = booking.BookingSeats.Select(s => s.SeatId).ToHashSet();
            if (show.Id == booking.ShowTimeId)
                taken.ExceptWith(oldSeatIds);
            if (seatIds.Any(seatId => taken.Contains(seatId)))
                throw new InvalidOperationException("Ghế mới đã có người đặt.");

            var snack = booking.Concessions.Sum(c => c.UnitPrice * c.Quantity);
            var quote = CinemaPricing.Quote(
                _context, show.Id, seatIds.Count, snack, booking.PromoCode, false, booking.UserId, booking.Id);
            if (!quote.Ok)
                throw new InvalidOperationException(quote.Message);

            var oldTotal = booking.TotalPrice;
            var oldShowId = booking.ShowTimeId;
            var oldTickets = _context.Tickets
                .Where(t => t.ShowTimeId == oldShowId && oldSeatIds.Contains(t.SeatId))
                .ToList();
            _context.Tickets.RemoveRange(oldTickets);
            _context.ElectronicTickets.RemoveRange(booking.ElectronicTickets);
            _context.BookingSeats.RemoveRange(booking.BookingSeats);
            _context.SaveChanges();

            booking.ShowTimeId = show.Id;
            booking.TotalPrice = quote.Total;
            booking.DiscountAmount = quote.Discount;
            booking.SurchargeAmount = quote.Surcharge;
            booking.PromoCode = quote.Code ?? booking.PromoCode;

            var seatNumbers = _context.Seats.Where(s => seatIds.Contains(s.Id)).ToList();
            foreach (var seatId in seatIds)
            {
                booking.BookingSeats.Add(new BookingSeat { SeatId = seatId, Price = quote.SeatPrice });
                if (booking.Status == BookingStatus.Paid)
                {
                    _context.Tickets.Add(new Ticket
                    {
                        ShowTimeId = show.Id,
                        SeatId = seatId,
                        Price = quote.SeatPrice
                    });
                    var qr = $"BK{booking.Id}-S{seatId}-{Guid.NewGuid():N}";
                    _context.ElectronicTickets.Add(new ElectronicTicket
                    {
                        BookingId = booking.Id,
                        SeatId = seatId,
                        QRCode = qr
                    });
                }
            }

            var difference = quote.Total - oldTotal;
            if (booking.Status == BookingStatus.Paid && difference > 0 && staffId > 0)
            {
                _context.Payments.Add(new Payment
                {
                    BookingId = booking.Id,
                    Amount = difference,
                    Method = PaymentMethod.Cash,
                    Status = PaymentStatus.Success,
                    StaffId = staffId,
                    PaidAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    GatewayOrderId = $"EXCHANGE-{booking.Id}-{DateTime.UtcNow:yyyyMMddHHmmss}",
                    GatewayResponse = "Thu chênh lệch đổi vé"
                });
            }

            _context.SaveChanges();

            var message = difference > 0
                ? $"Đã đổi vé. Khách cần trả thêm {difference:N0}đ."
                : difference < 0
                    ? $"Đã đổi vé. Hoàn lại cho khách {Math.Abs(difference):N0}đ."
                    : "Đã đổi vé. Giá không đổi.";

            return new ExchangeBookingResult
            {
                BookingId = booking.Id,
                OldTotal = oldTotal,
                NewTotal = quote.Total,
                Difference = difference,
                Message = message,
                RoomName = show.Room.Name,
                ShowTimeStart = show.StartTime,
                SeatNumbers = seatNumbers.Select(s => s.SeatNumber).OrderBy(s => s).ToList()
            };
        }

        private CheckInResponse AdmitTicket(ElectronicTicket ticket)
        {
            var show = ticket.Booking.ShowTime;
            var room = show.Room?.Name ?? "phòng chiếu";
            var movie = show.Movie?.Title ?? "";
            var seat = ticket.Seat?.SeatNumber ?? "";
            var response = new CheckInResponse
            {
                BookingId = ticket.BookingId,
                MovieTitle = movie,
                RoomName = room,
                ShowTimeStart = show.StartTime,
                SeatNumber = seat,
                CheckedInAt = ticket.CheckedInAt ?? DateTime.UtcNow
            };

            if (ticket.Booking.Status != BookingStatus.Paid)
            {
                response.Tone = "unpaid";
                response.Message = "Vé chưa thanh toán, không cho vào.";
                return response;
            }

            var showDay = CinemaPricing.ToVietnam(show.StartTime).Date;
            var today = CinemaPricing.ToVietnam(DateTime.UtcNow).Date;
            if (showDay != today)
            {
                response.Tone = "wrong";
                response.Message = $"Vé sai suất chiếu. Suất này là {showDay:dd/MM/yyyy} lúc {CinemaPricing.ToVietnam(show.StartTime):HH:mm}, phòng {room}.";
                return response;
            }

            if (ticket.CheckedInAt != null)
            {
                response.Tone = "used";
                response.CheckedInAt = ticket.CheckedInAt.Value;
                response.Message = "Vé đã sử dụng.";
                return response;
            }

            ticket.CheckedInAt = DateTime.UtcNow;
            _context.SaveChanges();
            response.CheckedInAt = ticket.CheckedInAt.Value;
            response.Tone = "ok";
            response.Message = $"Vé hợp lệ, mời khách vào phòng {room}.";
            return response;
        }

        private AdminShiftDto MapShift(StaffShift shift, Dictionary<int, User> people, bool withLines)
        {
            people.TryGetValue(shift.StaffId, out var staff);
            var totals = ShiftTotals(shift);
            var expectedCash = shift.ExpectedCash ?? totals.Cash;
            var expectedTransfer = shift.ExpectedTransfer ?? totals.Transfer;
            var status = shift.ApprovedAt != null
                ? "Approved"
                : shift.EndedAt == null
                    ? "Open"
                    : shift.AwaitingHandover && shift.ReceivedAt == null
                        ? "Handover"
                        : "Closed";
            var dto = new AdminShiftDto
            {
                Id = shift.Id,
                StaffId = shift.StaffId,
                StaffName = staff?.FullName ?? "Nhân viên",
                StaffEmail = staff?.Email ?? "",
                StartedAt = UtcStamp(shift.StartedAt),
                EndedAt = shift.EndedAt == null ? null : UtcStamp(shift.EndedAt.Value),
                Status = status,
                ExpectedCash = expectedCash,
                ExpectedTransfer = expectedTransfer,
                CountedCash = shift.CountedCash,
                CashDifference = shift.CountedCash == null ? null : shift.CountedCash.Value - expectedCash,
                TicketCount = totals.Tickets,
                BookingCount = totals.Bookings,
                Note = shift.Note,
                ApprovedAt = shift.ApprovedAt == null ? null : UtcStamp(shift.ApprovedAt.Value)
            };
            if (withLines)
            {
                var end = shift.EndedAt ?? DateTime.UtcNow;
                dto.Lines = BuildShiftReport(shift, shift.StaffId, end).Lines;
            }
            return dto;
        }

        private StaffShift EnsureOpenShift(int staffId)
        {
            var open = _context.StaffShifts.FirstOrDefault(s => s.StaffId == staffId && s.EndedAt == null);
            if (open != null) return open;
            open = new StaffShift { StaffId = staffId, StartedAt = DateTime.UtcNow };
            _context.StaffShifts.Add(open);
            _context.SaveChanges();
            return open;
        }

        private (decimal Cash, decimal Transfer, int Tickets, int Bookings) ShiftTotals(StaffShift shift)
        {
            var totals = TodayCollected();
            return (totals.Cash, totals.Transfer, totals.Tickets, totals.Bookings);
        }

        private (decimal Cash, decimal Transfer, int Tickets, int Bookings, List<Payment> Payments) TodayCollected()
            => CollectedDuring(VietnamDayUtc(DateTime.UtcNow));

        private (decimal Cash, decimal Transfer, int Tickets, int Bookings, List<Payment> Payments) CollectedDuring((DateTime StartUtc, DateTime EndUtc) window)
        {
            var payments = _context.Payments
                .Where(p => p.Status == PaymentStatus.Success && p.PaidAt != null && p.PaidAt >= window.StartUtc && p.PaidAt < window.EndUtc)
                .OrderBy(p => p.PaidAt)
                .ToList();
            var cash = payments.Where(p => p.Method == PaymentMethod.Cash).Sum(p => p.Amount);
            var transfer = payments.Where(p => p.Method != PaymentMethod.Cash).Sum(p => p.Amount);
            var bookingIds = payments.Select(p => p.BookingId).Distinct().ToList();
            var tickets = bookingIds.Count == 0
                ? 0
                : _context.BookingSeats.Count(s => bookingIds.Contains(s.BookingId));
            return (cash, transfer, tickets, bookingIds.Count, payments);
        }

        private ShiftReportDto ReadOrBuildReport(StaffShift shift, int staffId, ref bool changed)
        {
            if (!string.IsNullOrWhiteSpace(shift.ReportJson))
            {
                var saved = System.Text.Json.JsonSerializer.Deserialize<ShiftReportDto>(shift.ReportJson, ReportJsonOptions);
                if (saved != null) return saved;
            }

            var report = BuildShiftReport(shift, staffId, shift.EndedAt ?? DateTime.UtcNow);
            shift.ReportJson = System.Text.Json.JsonSerializer.Serialize(report);
            if (shift.CountedCash == null)
            {
                shift.ExpectedCash = report.CashTotal;
                shift.ExpectedTransfer = report.TransferTotal;
            }
            changed = true;
            return report;
        }

        private (DateTime StartUtc, DateTime EndUtc) VietnamDayUtc(DateTime utcInstant)
        {
            var utc = utcInstant.Kind == DateTimeKind.Utc
                ? utcInstant
                : DateTime.SpecifyKind(utcInstant, DateTimeKind.Utc);
            var local = TimeZoneInfo.ConvertTimeFromUtc(utc, CinemaPricing.Vietnam).Date;
            var start = TimeZoneInfo.ConvertTimeToUtc(
                DateTime.SpecifyKind(local, DateTimeKind.Unspecified),
                CinemaPricing.Vietnam);
            return (start, start.AddDays(1));
        }

        private static string PayLabel(PaymentMethod method) => method switch
        {
            PaymentMethod.Cash => "Tiền mặt",
            PaymentMethod.VNPay => "VNPay",
            PaymentMethod.Momo => "MoMo",
            PaymentMethod.BankTransfer => "Chuyển khoản",
            _ => "Chuyển khoản"
        };

        private ShiftReportDto BuildShiftReport(StaffShift shift, int staffId, DateTime endedAt)
        {
            var totals = CollectedDuring(VietnamDayUtc(endedAt));
            var payments = totals.Payments;
            var name = _context.Users.Where(u => u.Id == staffId).Select(u => u.FullName).FirstOrDefault() ?? "Nhân viên";
            var bookingIds = payments.Select(p => p.BookingId).Distinct().ToList();
            var bookings = _context.Bookings
                .Include(b => b.ShowTime).ThenInclude(s => s.Movie)
                .Include(b => b.BookingSeats)
                .Where(b => bookingIds.Contains(b.Id))
                .ToList()
                .ToDictionary(b => b.Id);

            return new ShiftReportDto
            {
                ShiftId = shift.Id,
                StaffName = name,
                StartedAt = UtcStamp(shift.StartedAt),
                EndedAt = UtcStamp(endedAt),
                CinemaRevenueToday = CinemaRevenueToday(),
                CashTotal = totals.Cash,
                TransferTotal = totals.Transfer,
                Total = totals.Cash + totals.Transfer,
                TicketCount = totals.Tickets,
                BookingCount = totals.Bookings,
                Lines = payments.Select(p => new ShiftLineDto
                {
                    BookingId = p.BookingId,
                    MovieTitle = bookings.TryGetValue(p.BookingId, out var booking) ? booking.ShowTime.Movie.Title : "",
                    Method = PayLabel(p.Method),
                    Amount = p.Amount,
                    Seats = bookings.TryGetValue(p.BookingId, out var row) ? row.BookingSeats.Count : 0
                }).ToList()
            };
        }

        private void EnsureSuccessPayment(Booking booking, int staffId)
        {
            var payment = _context.Payments
                .OrderByDescending(p => p.Id)
                .FirstOrDefault(p => p.BookingId == booking.Id);
            if (payment == null)
            {
                payment = new Payment
                {
                    BookingId = booking.Id,
                    Amount = booking.TotalPrice,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Payments.Add(payment);
            }

            if (payment.Status != PaymentStatus.Success)
            {
                payment.Status = PaymentStatus.Success;
                payment.PaidAt ??= DateTime.UtcNow;
                payment.Amount = booking.TotalPrice;
                if (payment.Method == default)
                    payment.Method = staffId > 0 ? PaymentMethod.Cash : PaymentMethod.BankTransfer;
            }

            if (staffId > 0 && payment.StaffId == null)
                payment.StaffId = staffId;
        }

        private decimal CinemaRevenueToday()
        {
            var (startUtc, endUtc) = VietnamTodayUtc();
            var paidTodayIds = _context.Payments
                .Where(p => p.Status == PaymentStatus.Success && p.PaidAt >= startUtc && p.PaidAt < endUtc)
                .Select(p => p.BookingId)
                .ToList();
            var createdTodayIds = _context.Bookings
                .Where(b => b.Status == BookingStatus.Paid && b.CreatedAt >= startUtc && b.CreatedAt < endUtc)
                .Select(b => b.Id)
                .ToList();
            var todayIds = paidTodayIds.Union(createdTodayIds).Distinct().ToList();
            return todayIds.Count == 0
                ? 0m
                : _context.Bookings.Where(b => todayIds.Contains(b.Id)).Sum(b => b.TotalPrice);
        }

        private static DateTime UtcStamp(DateTime value)
        {
            return value.Kind == DateTimeKind.Utc
                ? value
                : DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        private static (DateTime StartUtc, DateTime EndUtc) VietnamTodayUtc()
        {
            var local = CinemaPricing.ToVietnam(DateTime.UtcNow).Date;
            var start = TimeZoneInfo.ConvertTimeToUtc(
                DateTime.SpecifyKind(local, DateTimeKind.Unspecified),
                CinemaPricing.Vietnam);
            return (start, start.AddDays(1));
        }

        private static PaymentMethod ParsePayMethod(string? method, int staffId)
        {
            var value = (method ?? "").Trim().ToLowerInvariant();
            if (value is "transfer" or "bank" or "vnpay" or "chuyenkhoan") return PaymentMethod.BankTransfer;
            if (value is "momo") return PaymentMethod.Momo;
            if (value is "stripe") return PaymentMethod.Stripe;
            if (value is "cash" or "tienmat") return PaymentMethod.Cash;
            return staffId > 0 ? PaymentMethod.Cash : PaymentMethod.BankTransfer;
        }

        public RoomLayoutDto? GetRoomLayout(int roomId)
        {
            var room = _context.Rooms.FirstOrDefault(r => r.Id == roomId);
            if (room == null) return null;
            return new RoomLayoutDto
            {
                RoomId = room.Id,
                RoomName = room.Name,
                Seats = _context.Seats
                    .Where(s => s.RoomId == roomId)
                    .OrderBy(s => s.SeatNumber)
                    .Select(s => new AdminSeatDto
                    {
                        Id = s.Id,
                        SeatNumber = s.SeatNumber,
                        SeatType = (int)s.SeatType,
                        IsBlocked = s.IsBlocked
                    })
                    .ToList()
            };
        }

        public RoomLayoutDto? SaveRoomLayout(int roomId, UpdateSeatLayoutRequest request)
        {
            var room = _context.Rooms.FirstOrDefault(r => r.Id == roomId);
            if (room == null) return null;
            var map = (request.Seats ?? new List<AdminSeatDto>()).ToDictionary(s => s.Id);
            var seats = _context.Seats.Where(s => s.RoomId == roomId).ToList();
            foreach (var seat in seats)
            {
                if (!map.TryGetValue(seat.Id, out var dto)) continue;
                seat.SeatType = dto.SeatType switch
                {
                    1 => SeatType.Vip,
                    2 => SeatType.Couple,
                    _ => SeatType.Normal
                };
                seat.IsBlocked = dto.IsBlocked;
            }
            _context.SaveChanges();
            return GetRoomLayout(roomId);
        }

        public List<PromotionDto> GetPromotions()
        {
            return _context.Promotions.OrderByDescending(p => p.Id).Select(MapPromo).ToList();
        }

        public PromotionDto? GetPromotion(int id)
        {
            var p = _context.Promotions.FirstOrDefault(x => x.Id == id);
            return p == null ? null : MapPromo(p);
        }

        public PromotionDto CreatePromotion(SavePromotionRequest request)
        {
            var p = new Promotion();
            ApplyPromo(p, request);
            _context.Promotions.Add(p);
            _context.SaveChanges();
            return MapPromo(p);
        }

        public PromotionDto? UpdatePromotion(int id, SavePromotionRequest request)
        {
            var p = _context.Promotions.FirstOrDefault(x => x.Id == id);
            if (p == null) return null;
            ApplyPromo(p, request);
            _context.SaveChanges();
            return MapPromo(p);
        }

        public bool DeletePromotion(int id)
        {
            var p = _context.Promotions.FirstOrDefault(x => x.Id == id);
            if (p == null) return false;
            _context.Promotions.Remove(p);
            _context.SaveChanges();
            return true;
        }

        public QuotePromotionDto QuotePromotion(QuotePromotionRequest request)
        {
            return CinemaPricing.Quote(
                _context,
                request.ShowtimeId,
                request.SeatCount,
                request.SnackTotal,
                request.Code,
                false,
                request.UserId);
        }

        public ReportDto GetReport(DateTime from, DateTime to, string groupBy)
        {
            if (to < from) (from, to) = (to, from);
            to = to.Date.AddDays(1);
            var group = (groupBy ?? "movie").ToLowerInvariant();
            var paid = _context.Bookings
                .Include(b => b.ShowTime).ThenInclude(s => s.Movie)
                .Include(b => b.ShowTime).ThenInclude(s => s.Room)
                .Include(b => b.BookingSeats)
                .Where(b => b.Status == BookingStatus.Paid && b.CreatedAt >= from && b.CreatedAt < to)
                .ToList();

            IEnumerable<IGrouping<string, Booking>> groups = group switch
            {
                "room" => paid.GroupBy(b => b.ShowTime.Room.Name),
                "hour" => paid.GroupBy(b => CinemaPricing.ToVietnam(b.ShowTime.StartTime).ToString("HH:00")),
                _ => paid.GroupBy(b => b.ShowTime.Movie.Title)
            };

            var rows = groups
                .Select(g => new ReportRowDto
                {
                    Label = g.Key,
                    Bookings = g.Count(),
                    Seats = g.Sum(b => b.BookingSeats.Count),
                    Revenue = g.Sum(b => b.TotalPrice)
                })
                .OrderByDescending(r => r.Revenue)
                .ToList();

            return new ReportDto
            {
                From = from,
                To = to.AddDays(-1),
                GroupBy = group,
                TotalRevenue = rows.Sum(r => r.Revenue),
                TotalBookings = rows.Sum(r => r.Bookings),
                TotalSeats = rows.Sum(r => r.Seats),
                Rows = rows
            };
        }

        public byte[] ExportReportExcel(DateTime from, DateTime to, string groupBy)
        {
            var report = GetReport(from, to, groupBy);
            var title = report.GroupBy switch
            {
                "room" => "Doanh thu theo phòng",
                "hour" => "Doanh thu theo khung giờ",
                _ => "Doanh thu theo phim"
            };
            var sb = new System.Text.StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.Append("<?mso-application progid=\"Excel.Sheet\"?>");
            sb.Append("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\">");
            sb.Append("<Worksheet ss:Name=\"Bao cao\"><Table>");
            sb.Append($"<Row><Cell><Data ss:Type=\"String\">{Xml(title)}</Data></Cell></Row>");
            sb.Append($"<Row><Cell><Data ss:Type=\"String\">Từ {report.From:dd/MM/yyyy} đến {report.To:dd/MM/yyyy}</Data></Cell></Row>");
            sb.Append("<Row></Row>");
            sb.Append("<Row><Cell><Data ss:Type=\"String\">Nhóm</Data></Cell><Cell><Data ss:Type=\"String\">Số đơn</Data></Cell><Cell><Data ss:Type=\"String\">Số ghế</Data></Cell><Cell><Data ss:Type=\"String\">Doanh thu</Data></Cell></Row>");
            foreach (var row in report.Rows)
            {
                sb.Append("<Row>");
                sb.Append($"<Cell><Data ss:Type=\"String\">{Xml(row.Label)}</Data></Cell>");
                sb.Append($"<Cell><Data ss:Type=\"Number\">{row.Bookings}</Data></Cell>");
                sb.Append($"<Cell><Data ss:Type=\"Number\">{row.Seats}</Data></Cell>");
                sb.Append($"<Cell><Data ss:Type=\"Number\">{row.Revenue}</Data></Cell>");
                sb.Append("</Row>");
            }
            sb.Append("<Row></Row>");
            sb.Append($"<Row><Cell><Data ss:Type=\"String\">Tổng</Data></Cell><Cell><Data ss:Type=\"Number\">{report.TotalBookings}</Data></Cell><Cell><Data ss:Type=\"Number\">{report.TotalSeats}</Data></Cell><Cell><Data ss:Type=\"Number\">{report.TotalRevenue}</Data></Cell></Row>");
            sb.Append("</Table></Worksheet></Workbook>");
            return System.Text.Encoding.UTF8.GetBytes(sb.ToString());
        }

        public SystemSettingsDto GetSettings() => CinemaPricing.LoadSettings(_context);

        public SystemSettingsDto SaveSettings(SystemSettingsDto request)
        {
            if (request.SeatPrice < 0) request.SeatPrice = 0;
            CinemaPricing.SaveSettings(_context, request);
            return CinemaPricing.LoadSettings(_context);
        }

        private List<RevenuePointDto> BuildRevenueByDay()
        {
            var today = CinemaPricing.ToVietnam(DateTime.UtcNow).Date;
            var start = today.AddDays(-13);
            var paid = _context.Bookings
                .Where(b => b.Status == BookingStatus.Paid)
                .Select(b => new
                {
                    b.Id,
                    b.CreatedAt,
                    b.TotalPrice,
                    Movie = b.ShowTime.Movie.Title,
                    Room = b.ShowTime.Room.Name,
                    Start = b.ShowTime.StartTime,
                    Seats = b.BookingSeats.Count
                })
                .ToList();
            return Enumerable.Range(0, 14).Select(i =>
            {
                var day = start.AddDays(i);
                var ofDay = paid.Where(b => CinemaPricing.ToVietnam(b.CreatedAt).Date == day).ToList();
                return new RevenuePointDto
                {
                    Date = day.ToString("dd/MM"),
                    Revenue = ofDay.Sum(b => b.TotalPrice),
                    Bookings = ofDay.Count,
                    Lines = ofDay.OrderByDescending(b => b.TotalPrice).Select(b => new RevenueLineDto
                    {
                        BookingId = b.Id,
                        MovieTitle = b.Movie,
                        RoomName = b.Room,
                        ShowTimeStart = b.Start,
                        Seats = b.Seats,
                        TotalPrice = b.TotalPrice
                    }).ToList()
                };
            }).ToList();
        }

        private List<RoomOccupancyDto> BuildOccupancy()
        {
            var now = DateTime.UtcNow.AddDays(-7);
            var shows = _context.ShowTimes
                .Select(s => new { s.Id, s.RoomId, RoomName = s.Room.Name, Movie = s.Movie.Title, s.StartTime })
                .Where(s => s.StartTime >= now)
                .ToList();
            var seatCounts = _context.Seats
                .Where(s => !s.IsBlocked)
                .GroupBy(s => s.RoomId)
                .Select(g => new { RoomId = g.Key, Count = g.Count() })
                .ToDictionary(x => x.RoomId, x => x.Count);
            var showIds = shows.Select(s => s.Id).ToList();
            var soldRows = showIds.Count == 0
                ? new List<(int ShowTimeId, int Count)>()
                : _context.Tickets
                    .Where(t => showIds.Contains(t.ShowTimeId))
                    .GroupBy(t => t.ShowTimeId)
                    .Select(g => new { ShowTimeId = g.Key, Count = g.Count() })
                    .AsEnumerable()
                    .Select(g => (g.ShowTimeId, g.Count))
                    .ToList();
            var soldMap = soldRows.ToDictionary(r => r.ShowTimeId, r => r.Count);
            var sold = shows
                .GroupBy(s => s.RoomId)
                .ToDictionary(g => g.Key, g => g.Sum(s => soldMap.TryGetValue(s.Id, out var n) ? n : 0));

            return _context.Rooms.OrderBy(r => r.Name).AsEnumerable().Select(room =>
            {
                var roomShows = shows.Where(s => s.RoomId == room.Id).OrderBy(s => s.StartTime).ToList();
                var seatCount = seatCounts.TryGetValue(room.Id, out var seats) ? seats : 0;
                var capacity = seatCount * roomShows.Count;
                var filled = sold.TryGetValue(room.Id, out var n) ? n : 0;
                return new RoomOccupancyDto
                {
                    RoomId = room.Id,
                    RoomName = room.Name,
                    Capacity = capacity,
                    SoldSeats = filled,
                    Rate = capacity == 0 ? 0 : Math.Round(100m * filled / capacity, 1),
                    Shows = roomShows.Select(s =>
                    {
                        var soldSeats = soldMap.TryGetValue(s.Id, out var c) ? c : 0;
                        return new ShowFillDto
                        {
                            ShowTimeId = s.Id,
                            MovieTitle = s.Movie,
                            StartTime = s.StartTime,
                            Sold = soldSeats,
                            Capacity = seatCount,
                            Rate = seatCount == 0 ? 0 : Math.Round(100m * soldSeats / seatCount, 1)
                        };
                    }).ToList()
                };
            }).ToList();
        }

        private List<AlertDto> BuildAlerts()
        {
            var alerts = new List<AlertDto>();
            var settings = CinemaPricing.LoadSettings(_context);
            var low = _context.ConcessionItems.Where(c => c.IsActive && c.StockQuantity <= settings.LowStockAlert).ToList();
            foreach (var item in low)
            {
                alerts.Add(new AlertDto
                {
                    Level = item.StockQuantity == 0 ? "danger" : "warning",
                    Title = "Tồn bắp nước thấp",
                    Text = $"{item.Name} còn {item.StockQuantity}.",
                    Href = "/admin/concessions"
                });
            }

            var refunds = _context.Bookings.Count(b => b.Status == BookingStatus.Refunded);
            if (refunds > 0)
            {
                alerts.Add(new AlertDto
                {
                    Level = "warning",
                    Title = "Chờ hoàn tiền",
                    Text = $"{refunds} đơn đang chờ trả tiền cho khách.",
                    Href = "/admin/bookings"
                });
            }

            var stale = DateTime.UtcNow.AddMinutes(-20);
            var pending = _context.Bookings.Count(b => b.Status == BookingStatus.Pending && b.CreatedAt < stale);
            if (pending > 0)
            {
                alerts.Add(new AlertDto
                {
                    Level = "info",
                    Title = "Đơn chờ thanh toán",
                    Text = $"{pending} đơn pending hơn 20 phút.",
                    Href = "/admin/bookings"
                });
            }

            var emptyRooms = _context.Rooms
                .Where(r => !_context.Seats.Any(s => s.RoomId == r.Id && !s.IsBlocked))
                .Select(r => r.Name)
                .ToList();
            foreach (var name in emptyRooms)
            {
                alerts.Add(new AlertDto
                {
                    Level = "danger",
                    Title = "Phòng chưa có ghế",
                    Text = $"{name} không còn ghế bán được.",
                    Href = "/admin/rooms"
                });
            }

            return alerts;
        }

        private static PromotionDto MapPromo(Promotion p) => new()
        {
            Id = p.Id,
            Name = p.Name,
            Code = p.Code,
            Kind = p.Kind,
            Value = p.Value,
            Weekday = p.Weekday,
            MinSubtotal = p.MinSubtotal,
            MaxDiscount = p.MaxDiscount,
            UsageLimit = p.UsageLimit,
            UsedCount = p.UsedCount,
            StartsAt = p.StartsAt,
            EndsAt = p.EndsAt,
            AppliesTo = p.AppliesTo,
            Note = p.Note,
            IsActive = p.IsActive
        };

        private static void ApplyPromo(Promotion p, SavePromotionRequest req)
        {
            p.Name = InputGuard.Plain(req.Name, 120);
            p.Code = string.IsNullOrWhiteSpace(req.Code) ? null : InputGuard.Plain(req.Code.Trim().ToUpperInvariant(), 40);
            p.Kind = req.Kind is "Fixed" or "Weekday" ? req.Kind : "Percent";
            p.Value = req.Value < 0 ? 0 : req.Value;
            p.Weekday = p.Kind == "Weekday" ? req.Weekday : null;
            p.MinSubtotal = req.MinSubtotal < 0 ? 0 : req.MinSubtotal;
            p.MaxDiscount = req.MaxDiscount;
            p.UsageLimit = req.UsageLimit is int limit && limit > 0 ? limit : 1;
            p.StartsAt = req.StartsAt;
            p.EndsAt = req.EndsAt;
            p.AppliesTo = req.AppliesTo is "Ticket" or "Concession" ? req.AppliesTo : "All";
            p.Note = InputGuard.Optional(req.Note, 400);
            p.IsActive = req.IsActive;
        }

        private static string Xml(string? text)
        {
            return System.Security.SecurityElement.Escape(text ?? "") ?? "";
        }
    }
}