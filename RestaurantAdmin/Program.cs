using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RestaurantAdmin.Data;
using RestaurantAdmin.Models;

var builder = WebApplication.CreateBuilder(args);

// ─── Services ─────────────────────────────────────────────────────────────────

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 3;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.User.RequireUniqueEmail = false;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// ─── Middleware ────────────────────────────────────────────────────────────────

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// ─── Initialize Database ───────────────────────────────────────────────────────

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        // Open a raw connection to check if tables exist
        var conn = context.Database.GetDbConnection();
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AspNetRoles'";
        var result = await cmd.ExecuteScalarAsync();
        int tableCount = Convert.ToInt32(result);

        await conn.CloseAsync();

        if (tableCount == 0)
        {
            logger.LogInformation("Tables not found in database. Dropping and recreating schema...");
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();
            logger.LogInformation("Schema created successfully.");
        }
        else
        {
            logger.LogInformation("Database schema already exists. Checking for missing columns...");

            // Add new profile columns to AspNetUsers if they don't exist yet
            var conn2 = context.Database.GetDbConnection();
            if (conn2.State != System.Data.ConnectionState.Open)
                await conn2.OpenAsync();

            var profileColumns = new Dictionary<string, string>
            {
                ["FirstName"]        = "NVARCHAR(100) NULL",
                ["LastName"]         = "NVARCHAR(100) NULL",
                ["Address"]          = "NVARCHAR(200) NULL",
                ["City"]             = "NVARCHAR(100) NULL",
                ["Country"]          = "NVARCHAR(100) NULL",
                ["Gender"]           = "NVARCHAR(20) NULL",
                ["DateOfBirth"]      = "DATETIME2 NULL",
                ["Department"]       = "NVARCHAR(100) NULL",
                ["Position"]         = "NVARCHAR(100) NULL",
                ["ProfileNotes"]     = "NVARCHAR(1000) NULL",
                ["LastLoginAt"]      = "DATETIME2 NULL",
                ["ProfileImagePath"] = "NVARCHAR(500) NULL",
            };
            foreach (var col in profileColumns)
            {
                using var checkCmd = conn2.CreateCommand();
                checkCmd.CommandText = $@"
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_NAME = 'AspNetUsers' AND COLUMN_NAME = '{col.Key}'";
                var exists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

                if (exists == 0)
                {
                    using var alterCmd = conn2.CreateCommand();
                    alterCmd.CommandText = $"ALTER TABLE [AspNetUsers] ADD [{col.Key}] {col.Value}";
                    await alterCmd.ExecuteNonQueryAsync();
                    logger.LogInformation("Added missing column: AspNetUsers.{Column}", col.Key);
                }
            }

            // Add ImagePath to MenuItems if missing
            using var checkImg = conn2.CreateCommand();
            checkImg.CommandText = @"SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME = 'MenuItems' AND COLUMN_NAME = 'ImagePath'";
            var imgExists = Convert.ToInt32(await checkImg.ExecuteScalarAsync());
            if (imgExists == 0)
            {
                using var alterImg = conn2.CreateCommand();
                alterImg.CommandText = "ALTER TABLE [MenuItems] ADD [ImagePath] NVARCHAR(500) NULL";
                await alterImg.ExecuteNonQueryAsync();
                logger.LogInformation("Added missing column: MenuItems.ImagePath");
            }

            await conn2.CloseAsync();
            logger.LogInformation("Column check complete.");
        }

        await DbSeeder.SeedAsync(services, builder.Configuration);
        logger.LogInformation("Database seeded successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred initializing the database.");
        throw;
    }
}

app.Run();
