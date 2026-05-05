using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAdmin.Data;
using RestaurantAdmin.Models;

namespace RestaurantAdmin.Controllers;

[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        // Users
        ViewBag.TotalUsers     = await _userManager.Users.CountAsync();
        ViewBag.SuspendedUsers = await _userManager.Users.CountAsync(u => u.IsSuspended);

        // Menu
        ViewBag.TotalMenuItems  = await _context.MenuItems.CountAsync();
        ViewBag.TotalCategories = await _context.MenuCategories.CountAsync();
        ViewBag.ActiveMenuItems = await _context.MenuItems.CountAsync(m => m.IsAvailable);

        // Orders — handle case where Orders table may not exist yet
        try
        {
            var today = DateTime.UtcNow.Date;

            ViewBag.TotalOrders   = await _context.Orders.CountAsync();
            ViewBag.TodayOrders   = await _context.Orders.CountAsync(o => o.OrderDate >= today);
            ViewBag.TotalRevenue  = await _context.Orders.SumAsync(o => (decimal?)o.SubTotal) ?? 0m;
            ViewBag.TodayRevenue  = await _context.Orders
                .Where(o => o.OrderDate >= today)
                .SumAsync(o => (decimal?)o.SubTotal) ?? 0m;

            ViewBag.RecentOrders  = await _context.Orders
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();
        }
        catch
        {
            // Orders table doesn't exist yet — PosApi hasn't run
            ViewBag.TotalOrders  = 0;
            ViewBag.TodayOrders  = 0;
            ViewBag.TotalRevenue = 0m;
            ViewBag.TodayRevenue = 0m;
            ViewBag.RecentOrders = new List<Order>();
        }

        return View();
    }
}
