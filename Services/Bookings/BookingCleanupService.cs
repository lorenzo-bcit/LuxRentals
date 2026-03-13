using LuxRentals.Data;
using Microsoft.EntityFrameworkCore;

namespace LuxRentals.Services
{
    public class BookingCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BookingCleanupService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1); // Check every hour

        public BookingCleanupService(
            IServiceProvider serviceProvider,
            ILogger<BookingCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Booking Cleanup Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupOldBookingsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during booking cleanup.");
                }

                // Wait for the next check interval
                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Booking Cleanup Service stopped.");
        }

        private async Task CleanupOldBookingsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<LuxRentalsDbContext>();

            var now = DateTime.UtcNow;
            var cutoffTime = now.AddHours(-24); // 24 hours ago

            // Bookings to delete:
            // 1. Cancelled bookings older than 24 hours
            // 2. Completed bookings (end date + 24 hours has passed)
            var bookingsToDelete = await context.Bookings
                .Where(b =>
                    // Cancelled more than 24 hours ago
                    (b.CancelledAt != null && b.CancelledAt < cutoffTime) ||
                    // Completed more than 24 hours ago (end date + 24hrs)
                    (b.CancelledAt == null && b.EndDateTime.AddHours(24) < now)
                )
                .ToListAsync();

            if (bookingsToDelete.Any())
            {
                context.Bookings.RemoveRange(bookingsToDelete);
                await context.SaveChangesAsync();

                _logger.LogInformation(
                    "Deleted {Count} old bookings (cancelled or completed more than 24 hours ago).",
                    bookingsToDelete.Count
                );
            }
        }
    }
}