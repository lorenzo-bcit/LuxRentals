using Microsoft.EntityFrameworkCore;

namespace LuxRentals.Data;

public static class DatabaseMigrator
{
    public static async Task ApplyPendingMigrationsAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<LuxRentalsDbContext>();
        var loggerFactory = services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger(typeof(DatabaseMigrator));
        var pending = (await db.Database.GetPendingMigrationsAsync()).ToList();

        if (pending.Count == 0)
        {
            return;
        }

        logger.LogInformation("Applying {Count} pending migrations...", pending.Count);
        await db.Database.MigrateAsync();
        logger.LogInformation("Migrations applied.");
    }
}
