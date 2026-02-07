using Microsoft.AspNetCore.Identity;

namespace LuxRentals.Data;

public static class AdminSeeder
{
    private const string ADMIN_EMAIL = "admin@example.com";
    private const string ADMIN_PASSWORD = "Admin123!";
    private const string ADMIN_ROLE = "Admin";

    public static async Task SeedAsync(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        // ensure admin role exists
        if (!await roleManager.RoleExistsAsync(ADMIN_ROLE))
        {
            await roleManager.CreateAsync(new IdentityRole(ADMIN_ROLE));
        }

        // ensure at least one admin user exists
        var admins = await userManager.GetUsersInRoleAsync(ADMIN_ROLE);
        if (admins.Count == 0)
        {
            // no admin exists, create the default admin
            var adminUser = new IdentityUser
            {
                UserName = ADMIN_EMAIL,
                Email = ADMIN_EMAIL,
                EmailConfirmed = true
            };

            // could pull this from env var
            await userManager.CreateAsync(adminUser, ADMIN_PASSWORD);

            await userManager.AddToRoleAsync(adminUser, ADMIN_ROLE);
        }
    }
}
