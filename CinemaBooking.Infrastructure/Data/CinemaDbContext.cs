using CinemaBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Infrastructure.Data
{
    public class CinemaDbContext : DbContext
    {
        public CinemaDbContext(DbContextOptions<CinemaDbContext> options)
            : base(options) { }

        public DbSet<Movie> Movies => Set<Movie>();
        public DbSet<Room> Rooms => Set<Room>();
        public DbSet<Seat> Seats => Set<Seat>();
        public DbSet<ShowTime> ShowTimes => Set<ShowTime>();
        public DbSet<Ticket> Tickets => Set<Ticket>();
        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<BookingSeat> BookingSeats => Set<BookingSeat>();
        public DbSet<ElectronicTicket> ElectronicTickets => Set<ElectronicTicket>();
        public DbSet<Payment> Payments => Set<Payment>();

        public DbSet<User> Users => Set<User>();
        public DbSet<SeatHold> SeatHolds => Set<SeatHold>();
        public DbSet<HomeBlock> HomeBlocks => Set<HomeBlock>();
        public DbSet<ConcessionItem> ConcessionItems => Set<ConcessionItem>();
        public DbSet<BookingConcession> BookingConcessions => Set<BookingConcession>();
        public DbSet<SiteArticle> SiteArticles => Set<SiteArticle>();

        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);

            // Precision tiền tệ
            mb.Entity<Movie>().Property(m => m.Rating).HasPrecision(3, 1);
            mb.Entity<Booking>().Property(b => b.TotalPrice).HasPrecision(18, 2);
            mb.Entity<BookingSeat>().Property(b => b.Price).HasPrecision(18, 2);
            mb.Entity<Payment>().Property(p => p.Amount).HasPrecision(18, 2);

            // Table name
            mb.Entity<Room>().ToTable("Room");

            // ===== FIX CASCADE PATH =====
            // BookingSeat → Booking: CASCADE (xóa booking → xóa booking seat, OK)
            mb.Entity<BookingSeat>()
                .HasOne(bs => bs.Booking)
                .WithMany(b => b.BookingSeats)
                .HasForeignKey(bs => bs.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // BookingSeat → Seat: NO ACTION (tránh multiple cascade path)
            mb.Entity<BookingSeat>()
                .HasOne(bs => bs.Seat)
                .WithMany()
                .HasForeignKey(bs => bs.SeatId)
                .OnDelete(DeleteBehavior.NoAction);

            // ElectronicTicket → Booking: CASCADE
            mb.Entity<ElectronicTicket>()
                .HasOne(et => et.Booking)
                .WithMany(b => b.ElectronicTickets)
                .HasForeignKey(et => et.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // ElectronicTicket → Seat: NO ACTION
            mb.Entity<ElectronicTicket>()
                .HasOne(et => et.Seat)
                .WithMany()
                .HasForeignKey(et => et.SeatId)
                .OnDelete(DeleteBehavior.NoAction);

            // Payment → Booking: CASCADE
            mb.Entity<Payment>()
                .HasOne(p => p.Booking)
                .WithMany()
                .HasForeignKey(p => p.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seat → Room: NO ACTION (tránh cascade từ Room)
            mb.Entity<Seat>()
                .HasOne(s => s.Room)
                .WithMany()
                .HasForeignKey(s => s.RoomId)
                .OnDelete(DeleteBehavior.NoAction);

            mb.Entity<User>(e =>
            {
                e.HasIndex(u => u.Email).IsUnique();
                e.Property(u => u.Email).HasMaxLength(256).IsRequired();
                e.Property(u => u.FullName).HasMaxLength(128).IsRequired();
                e.Property(u => u.PasswordHash).HasMaxLength(512).IsRequired();
                e.Property(u => u.Phone).HasMaxLength(20);
            });

            // Một ghế của một suất chỉ có một lượt giữ. Hết hạn thì xóa dòng.
            mb.Entity<SeatHold>(e =>
            {
                e.HasIndex(h => new { h.ShowTimeId, h.SeatId }).IsUnique();
                e.HasIndex(h => h.HoldToken);

                e.HasOne(h => h.ShowTime)
                    .WithMany()
                    .HasForeignKey(h => h.ShowTimeId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(h => h.Seat)
                    .WithMany()
                    .HasForeignKey(h => h.SeatId)
                    .OnDelete(DeleteBehavior.NoAction);

                e.HasOne(h => h.User)
                    .WithMany()
                    .HasForeignKey(h => h.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            mb.Entity<HomeBlock>(e =>
            {
                e.Property(b => b.Kind).HasMaxLength(20).IsRequired();
                e.Property(b => b.Title).HasMaxLength(200).IsRequired();
                e.Property(b => b.Subtitle).HasMaxLength(300);
                e.Property(b => b.Tag).HasMaxLength(200);
                e.Property(b => b.DateLabel).HasMaxLength(40);
                e.Property(b => b.ImageUrl).HasMaxLength(500);
                e.Property(b => b.AccentColor).HasMaxLength(20);
                e.HasData(
                    new HomeBlock
                    {
                        Id = 1,
                        Kind = "hero",
                        Title = "TRẠI BUÔN NGƯỜI",
                        Subtitle = "THE JOURNEY OF HEROES",
                        Tag = "A FILM BY TONI DƯƠNG BẢO ANH & MÈO BÍCH TRANG",
                        DateLabel = "25.09.2026",
                        ImageUrl = "https://images.unsplash.com/photo-1489599849927-2ee91cede3ba?w=1600",
                        MovieId = 1,
                        SortOrder = 1,
                        IsActive = true
                    },
                    new HomeBlock
                    {
                        Id = 2,
                        Kind = "hero",
                        Title = "HỒN ĐÁO QUÊN LÀNG",
                        Subtitle = "PHIM HOẠT HÌNH VIỆT NAM",
                        Tag = "SUẤT CHIẾU ĐẶC BIỆT",
                        DateLabel = "25.09.2026",
                        ImageUrl = "https://images.unsplash.com/photo-1478720568477-152d9b164e26?w=1600",
                        MovieId = 1,
                        SortOrder = 2,
                        IsActive = true
                    },
                    new HomeBlock
                    {
                        Id = 3,
                        Kind = "snack",
                        Title = "COMBO BẮP NƯỚC",
                        Subtitle = "Đồng giá 79K",
                        AccentColor = "#e63946",
                        SortOrder = 1,
                        IsActive = true
                    },
                    new HomeBlock
                    {
                        Id = 4,
                        Kind = "snack",
                        Title = "THIS OR THAT",
                        Subtitle = "Chọn combo yêu thích",
                        AccentColor = "#3b82f6",
                        SortOrder = 2,
                        IsActive = true
                    },
                    new HomeBlock
                    {
                        Id = 5,
                        Kind = "snack",
                        Title = "SPECIAL MONDAY",
                        Subtitle = "Đồng giá 50.000đ/vé 2D",
                        AccentColor = "#f59e0b",
                        SortOrder = 3,
                        IsActive = true
                    }
                );
            });

            mb.Entity<ConcessionItem>(e =>
            {
                e.Property(c => c.Name).HasMaxLength(120).IsRequired();
                e.Property(c => c.Description).HasMaxLength(300);
                e.Property(c => c.Price).HasPrecision(18, 2);
                e.Property(c => c.ImageUrl).HasMaxLength(500);
                e.Property(c => c.Category).HasMaxLength(40).IsRequired();
                e.HasData(
                    new ConcessionItem
                    {
                        Id = 1,
                        Name = "Bắp rang bơ",
                        Description = "Bắp ngọt, phần vừa",
                        Price = 45000m,
                        Category = "Bắp",
                        ImageUrl = "https://images.unsplash.com/photo-1578849278619-e73505e9610f?auto=format&fit=crop&w=600&q=80",
                        SortOrder = 1,
                        StockQuantity = 40,
                        IsActive = true
                    },
                    new ConcessionItem
                    {
                        Id = 2,
                        Name = "Bắp caramel",
                        Description = "Bắp caramel giòn",
                        Price = 55000m,
                        Category = "Bắp",
                        ImageUrl = "https://images.unsplash.com/photo-1585647347483-22b66260dfff?auto=format&fit=crop&w=600&q=80",
                        SortOrder = 2,
                        StockQuantity = 40,
                        IsActive = true
                    },
                    new ConcessionItem
                    {
                        Id = 3,
                        Name = "Nước ngọt",
                        Description = "Ly lớn, đá riêng",
                        Price = 25000m,
                        Category = "Nước",
                        ImageUrl = "https://images.unsplash.com/photo-1629203851122-3726ecdf080e?auto=format&fit=crop&w=600&q=80",
                        SortOrder = 3,
                        StockQuantity = 40,
                        IsActive = true
                    },
                    new ConcessionItem
                    {
                        Id = 4,
                        Name = "Nước suối",
                        Description = "Chai 500ml",
                        Price = 15000m,
                        Category = "Nước",
                        ImageUrl = "https://images.unsplash.com/photo-1548839140-29a749e1cf4d?auto=format&fit=crop&w=600&q=80",
                        SortOrder = 4,
                        StockQuantity = 40,
                        IsActive = true
                    },
                    new ConcessionItem
                    {
                        Id = 5,
                        Name = "Combo bắp + nước",
                        Description = "1 bắp rang bơ và 1 nước ngọt",
                        Price = 79000m,
                        Category = "Combo",
                        ImageUrl = "https://images.unsplash.com/photo-1513558161293-cdaf765ed2fd?auto=format&fit=crop&w=600&q=80",
                        SortOrder = 5,
                        StockQuantity = 40,
                        IsActive = true
                    }
                );
            });

            mb.Entity<BookingConcession>(e =>
            {
                e.Property(c => c.ItemName).HasMaxLength(120).IsRequired();
                e.Property(c => c.UnitPrice).HasPrecision(18, 2);
                e.HasOne(c => c.Booking)
                    .WithMany(b => b.Concessions)
                    .HasForeignKey(c => c.BookingId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(c => c.ConcessionItem)
                    .WithMany()
                    .HasForeignKey(c => c.ConcessionItemId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            mb.Entity<SiteArticle>(e =>
            {
                e.Property(a => a.Page).HasMaxLength(20).IsRequired();
                e.Property(a => a.Slot).HasMaxLength(20).IsRequired();
                e.Property(a => a.Title).HasMaxLength(200).IsRequired();
                e.Property(a => a.Meta).HasMaxLength(160);
                e.Property(a => a.Body).HasMaxLength(2000);
                e.Property(a => a.Extra).HasMaxLength(200);
                e.Property(a => a.ImageUrl).HasMaxLength(500);
                e.HasData(
                    Article(1, "news", "intro", "Mở đầu", null, "Thông báo vận hành rạp và các thay đổi trên hệ thống đặt vé.", null, 0),
                    Article(2, "news", "card", "National Cinema mở thêm suất cuối tuần", "12.10.2026", "Từ thứ Sáu, phòng 1 và phòng 2 tăng suất tối để phục vụ khách đặt vé online. Lịch chi tiết nằm ở mục Lịch chiếu.", null, 1),
                    Article(3, "news", "card", "Vé điện tử có mã QR check-in", "05.10.2026", "Sau khi thanh toán, vé nằm trong mục Vé của tôi. Nhân viên quét QR tại cửa phòng chiếu, không cần in giấy.", null, 2),
                    Article(4, "news", "card", "Ghế đôi đã có trên sơ đồ phòng", "28.09.2026", "Hàng ghế đôi được chọn theo cặp. Nếu một ghế trong cặp đã bán, cả cặp sẽ không chọn được.", null, 3),

                    Article(5, "promotion", "intro", "Mở đầu", null, "Ưu đãi đang công bố. Chỉ mức giá ghi trên màn thanh toán mới là số tiền của đơn online.", null, 0),
                    Article(6, "promotion", "card", "Đồng giá vé 75.000đ", "ONLINE", "Mọi ghế trên web đang tính 75.000đ. Giá này đã áp dụng khi bạn xác nhận đặt vé.", null, 1),
                    Article(7, "promotion", "card", "Combo bắp nước 79.000đ", "QUẦY", "Mua kèm vé tại quầy. Combo không trừ vào đơn thanh toán online.", null, 2),
                    Article(8, "promotion", "card", "Suất ngày thường tại quầy", "THỨ HAI", "Một số suất 2D bán tại quầy có giá ưu đãi. Nhân viên thông báo trước khi thu tiền.", null, 3),

                    Article(9, "price", "intro", "Mở đầu", null, "Giá trên website là giá đã gồm ghế bạn chọn. Đơn online chưa áp dụng mã giảm giá.", null, 0),
                    Article(10, "price", "row", "Thường, VIP, ghế đôi", "Đặt online", null, "75.000đ / ghế", 1),
                    Article(11, "price", "row", "Bán tại quầy", "Tiền mặt, chuyển khoản, thẻ", null, "Nhân viên xác nhận khi thu tiền", 2),
                    Article(12, "price", "card", "Giữ ghế", null, "Sau khi chọn ghế, hệ thống giữ trong 10 phút. Hết giờ ghế được mở lại cho khách khác.", null, 3),
                    Article(13, "price", "card", "Số ghế mỗi đơn", null, "Tối đa 8 ghế cho một lần đặt, kể cả ghế đôi. Ghế đôi tính theo từng chỗ ngồi.", null, 4),

                    Article(14, "festival", "intro", "Mở đầu", null, "Lịch sự kiện của National Cinema Center. Suất nào mở bán online sẽ xuất hiện trong Lịch chiếu.", null, 0),
                    Article(15, "festival", "card", "Tuần phim cuối tháng", "20–26.10.2026", "Các suất tối trong tuần được xếp thêm tại phòng 1. Xem giờ cụ thể ở Lịch chiếu.", null, 1),
                    Article(16, "festival", "card", "Tuần phim hoạt hình", "02–08.11.2026", "Suất sáng dành cho gia đình. Vé vẫn đặt trên web theo giá niêm yết.", null, 2),
                    Article(17, "festival", "card", "Suất đặc biệt tại rạp", "Cả năm", "Một số buổi giới thiệu phim chỉ bán tại quầy. Nhân viên sẽ thông báo khi suất không mở online.", null, 3),

                    Article(18, "about", "intro", "Mở đầu", null, "National Cinema Center là rạp đặt vé trực tuyến và bán vé tại quầy. Khách xem lịch, chọn ghế, thanh toán và nhận vé QR. Nhân viên check-in bằng mã trên vé.", null, 0),
                    Article(19, "about", "card", "Đặt vé online", null, "Chọn phim, suất chiếu và ghế. Ghế được giữ 10 phút trước khi thanh toán.", null, 1),
                    Article(20, "about", "card", "Quầy vé", null, "Nhân viên bán vé tại quầy, thu tiền và xuất lại mã QR nếu khách cần.", null, 2),
                    Article(21, "about", "card", "Liên hệ", null, "Hotline 1900 1234. Quầy mở trước suất chiếu đầu tiên trong ngày 30 phút.", null, 3)
                );
            });
        }

        private static SiteArticle Article(
            int id, string page, string slot, string title, string? meta, string? body, string? extra, int sort)
        {
            return new SiteArticle
            {
                Id = id,
                Page = page,
                Slot = slot,
                Title = title,
                Meta = meta,
                Body = body,
                Extra = extra,
                SortOrder = sort,
                IsActive = true
            };
        }
    }
}