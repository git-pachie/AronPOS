using Microsoft.AspNetCore.Identity;
using RestaurantAdmin.Models;

namespace RestaurantAdmin.Data;

public static class DbSeeder
{
    public static readonly string[] DefaultRoles = { "Admin", "Manager", "Staff" };

    public static async Task SeedAsync(
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Seed roles
        foreach (var role in DefaultRoles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Seed admin user
        var adminUsername = configuration["AdminSeed:Email"] ?? "admin";
        var adminPassword = configuration["AdminSeed:Password"] ?? "admin";

        var existingAdmin = await userManager.FindByNameAsync(adminUsername);
        if (existingAdmin == null)
        {
            var admin = new ApplicationUser
            {
                UserName = adminUsername,
                Email = adminUsername.Contains('@') ? adminUsername : $"{adminUsername}@localhost.com",
                FullName = "System Administrator",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(admin, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Failed to create admin user: {errors}");
            }
        }
        else
        {
            // Ensure existing admin has the Admin role
            if (!await userManager.IsInRoleAsync(existingAdmin, "Admin"))
                await userManager.AddToRoleAsync(existingAdmin, "Admin");
        }
    }
}
