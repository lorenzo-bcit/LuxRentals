using LuxRentals.Models;
using Microsoft.EntityFrameworkCore;

namespace LuxRentals.Data.Seeding
{
    public static class BookingStatusSeeder
    {
        public static async Task EnsureBookingStatusSeededAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<LuxRentalsDbContext>();

            // Check if already seeded
            if (await context.BookingStatuses.AnyAsync())
            {
                return; // Already has data
            }

            var statuses = new[]
            {
                new BookingStatus { BookingStatus1 = "Unbooked" },   // ID will be 1
                new BookingStatus { BookingStatus1 = "Booked" },     // ID will be 2
                new BookingStatus { BookingStatus1 = "Cancelled" }   // ID will be 3
            };

            await context.BookingStatuses.AddRangeAsync(statuses);
            await context.SaveChangesAsync();
        }
    }
}