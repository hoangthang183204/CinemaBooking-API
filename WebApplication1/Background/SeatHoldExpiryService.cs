using CinemaBooking.Domain.Enums;
using CinemaBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.API.Background
{
    /// <summary>
    /// Mỗi phút xóa ghế giữ tạm đã hết hạn và hủy đơn Pending quá 10 phút.
    /// </summary>
    public class SeatHoldExpiryService : BackgroundService
    {
        private static readonly TimeSpan ScanInterval = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan PaymentWindow = TimeSpan.FromMinutes(10);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SeatHoldExpiryService> _logger;

        public SeatHoldExpiryService(IServiceScopeFactory scopeFactory, ILogger<SeatHoldExpiryService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<CinemaDbContext>();
                    await ReleaseExpiredHolds(db, stoppingToken);
                    await CancelUnpaidBookings(db, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Quét đơn hết hạn thanh toán không xong, sẽ thử lại sau 1 phút.");
                }

                await Task.Delay(ScanInterval, stoppingToken);
            }
        }

        private static async Task ReleaseExpiredHolds(CinemaDbContext db, CancellationToken stoppingToken)
        {
            var now = DateTime.UtcNow;
            var expired = await db.SeatHolds
                .Where(h => h.ExpiresAt <= now)
                .ToListAsync(stoppingToken);
            if (expired.Count == 0) return;
            db.SeatHolds.RemoveRange(expired);
            await db.SaveChangesAsync(stoppingToken);
        }

        private async Task CancelUnpaidBookings(CinemaDbContext db, CancellationToken stoppingToken)
        {
            var cutoff = DateTime.UtcNow.Subtract(PaymentWindow);
            var ids = await db.Bookings
                .Where(b => b.Status == BookingStatus.Pending && b.CreatedAt < cutoff)
                .Where(b => !db.Payments.Any(p => p.BookingId == b.Id && p.Status == PaymentStatus.Success))
                .Select(b => b.Id)
                .ToListAsync(stoppingToken);

            foreach (var id in ids)
            {
                if (stoppingToken.IsCancellationRequested) return;
                try
                {
                    await CancelOne(db, id, cutoff, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Không hủy được đơn #{BookingId} quá hạn thanh toán.", id);
                    db.ChangeTracker.Clear();
                }
            }
        }

        private async Task CancelOne(CinemaDbContext db, int id, DateTime cutoff, CancellationToken stoppingToken)
        {
            await using var tx = await db.Database.BeginTransactionAsync(stoppingToken);
            var pending = (int)BookingStatus.Pending;
            var cancelled = (int)BookingStatus.Cancelled;
            var paid = (int)PaymentStatus.Success;
            var changed = await db.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE Bookings
                SET Status = {cancelled}
                WHERE Id = {id}
                  AND Status = {pending}
                  AND CreatedAt < {cutoff}
                  AND NOT EXISTS (
                      SELECT 1 FROM Payments WHERE BookingId = {id} AND Status = {paid}
                  )", stoppingToken);
            if (changed == 0)
            {
                await tx.RollbackAsync(stoppingToken);
                return;
            }

            var booking = await db.Bookings.FirstAsync(b => b.Id == id, stoppingToken);
            var lines = await db.BookingConcessions.Where(c => c.BookingId == id).ToListAsync(stoppingToken);
            var itemIds = lines.Select(c => c.ConcessionItemId).Distinct().ToList();
            var stock = await db.ConcessionItems.Where(c => itemIds.Contains(c.Id)).ToListAsync(stoppingToken);
            foreach (var line in lines)
            {
                var item = stock.FirstOrDefault(c => c.Id == line.ConcessionItemId);
                if (item != null) item.StockQuantity += line.Quantity;
            }

            if (!string.IsNullOrWhiteSpace(booking.PromoCode))
            {
                var promo = await db.Promotions.FirstOrDefaultAsync(p => p.Code == booking.PromoCode, stoppingToken);
                if (promo != null && promo.UsedCount > 0) promo.UsedCount -= 1;
            }

            var seatIds = await db.BookingSeats
                .Where(s => s.BookingId == id)
                .Select(s => s.SeatId)
                .ToListAsync(stoppingToken);
            var tickets = await db.Tickets
                .Where(t => t.ShowTimeId == booking.ShowTimeId && seatIds.Contains(t.SeatId))
                .ToListAsync(stoppingToken);
            db.Tickets.RemoveRange(tickets);
            var electronic = await db.ElectronicTickets.Where(t => t.BookingId == id).ToListAsync(stoppingToken);
            db.ElectronicTickets.RemoveRange(electronic);
            var holds = await db.SeatHolds
                .Where(h => h.ShowTimeId == booking.ShowTimeId && seatIds.Contains(h.SeatId))
                .ToListAsync(stoppingToken);
            db.SeatHolds.RemoveRange(holds);

            var payments = await db.Payments
                .Where(p => p.BookingId == id && p.Status == PaymentStatus.Pending)
                .ToListAsync(stoppingToken);
            foreach (var payment in payments)
            {
                payment.Status = PaymentStatus.Cancelled;
                payment.GatewayResponse = "Hết hạn sau 10 phút, chưa thanh toán";
            }

            await db.SaveChangesAsync(stoppingToken);
            await tx.CommitAsync(stoppingToken);
            _logger.LogInformation("Đã hủy đơn #{BookingId} vì quá 10 phút chưa thanh toán.", id);
        }
    }
}
