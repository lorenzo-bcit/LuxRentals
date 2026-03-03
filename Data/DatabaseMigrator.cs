using Microsoft.EntityFrameworkCore;

namespace LuxRentals.Data;

public static class DatabaseMigrator
{
    public static async Task ApplyPendingMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        var loggerFactory = services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger(typeof(DatabaseMigrator));

        var db = services.GetRequiredService<LuxRentalsDbContext>();

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