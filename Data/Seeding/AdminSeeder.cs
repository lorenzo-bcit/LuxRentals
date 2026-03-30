using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace LuxRentals.Data.Seeding;

public static class AdminSeeder
{
    private const string ADMIN_ROLE = "Admin";

    public static async Task EnsureAdminSeededAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        var loggerFactory = services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger(typeof(AdminSeeder));

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var options = services.GetRequiredService<IOptions<SeedDataOptions>>().Value;

        if (string.IsNullOrWhiteSpace(options.AdminEmail) || string.IsNullOrWhiteSpace(options.AdminPassword))
        {
            logger.LogWarning("Admin seeding skipped because SeedData:AdminEmail or SeedData:AdminPassword is missing.");
            return;
        }

        // ensure admin role exists
        if (!await roleManager.RoleExistsAsync(ADMIN_ROLE))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole(ADMIN_ROLE));
            if (!roleResult.Succeeded)
            {
                logger.LogError("Failed to create admin role.");
                return;
            }

            logger.LogInformation("Admin role created.");
        }

        // ensure at least one admin user exists
        var admins = await userManager.GetUsersInRoleAsync(ADMIN_ROLE);
        if (admins.Count > 0)
            return;

        var adminUser = new IdentityUser
        {
            UserName = options.AdminEmail,
            Email = options.AdminEmail,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(adminUser, options.AdminPassword);
        if (!createResult.Succeeded)
        {
            logger.LogError("Failed to create default admin user.");
            return;
        }

        logger.LogInformation("Default admin user created.");

        var addToRoleResult = await userManager.AddToRoleAsync(adminUser, ADMIN_ROLE);
        if (!addToRoleResult.Succeeded)
        {
            logger.LogError("Failed to add default admin user to role.");
            return;
        }

        logger.LogInformation("Default admin user added to Admin role.");
    }
}