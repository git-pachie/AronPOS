using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RestaurantAdmin.Models;

namespace RestaurantAdmin.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<MenuCategory> MenuCategories { get; set; }
    public DbSet<MenuItem> MenuItems { get; set; }
    public DbSet<MenuItemPriceHistory> MenuItemPriceHistories { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<MenuItem>()
            .HasOne(m => m.MenuCategory)
            .WithMany(c => c.MenuItems)
            .HasForeignKey(m => m.MenuCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<MenuItemPriceHistory>()
            .HasOne(h => h.MenuItem)
            .WithMany(m => m.PriceHistory)
            .HasForeignKey(h => h.MenuItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Order>()
            .HasMany(o => o.OrderItems)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Map to the exact table names created by PosApi
        builder.Entity<Order>().ToTable("Orders");
        builder.Entity<OrderItem>().ToTable("OrderItems");
    }
}
