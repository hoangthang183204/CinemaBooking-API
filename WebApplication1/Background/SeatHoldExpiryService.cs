using CinemaBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.API.Background
{
    /// <summary>
    /// Xóa SeatHold quá hạn. Không hủy Booking Pending của luồng cũ,
    /// vì luồng đó đã ghi Ticket ngay lúc tạo đơn.
    /// </summary>
    public class SeatHoldExpiryService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public SeatHoldExpiryService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<CinemaDbContext>();
                    var now = DateTime.UtcNow;
                    var expired = await db.SeatHolds
                        .Where(h => h.ExpiresAt <= now)
                        .ToListAsync(stoppingToken);

                    if (expired.Count > 0)
                    {
                        db.SeatHolds.RemoveRange(expired);
                        await db.SaveChangesAsync(stoppingToken);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch
                {
                    // Lần sau sẽ thử lại. Không làm chết process API.
                }

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
}
