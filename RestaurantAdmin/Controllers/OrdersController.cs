using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAdmin.Data;

namespace RestaurantAdmin.Controllers;

[Authorize(Roles = "Admin,Manager")]
public class OrdersController : Controller
{
    private readonly ApplicationDbContext _context;

    public OrdersController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Orders
    public async Task<IActionResult> Index(
        string? search,
        string? dateFrom,
        string? dateTo,
        int page = 1)
    {
        const int pageSize = 20;

        var query = _context.Orders
            .Include(o => o.OrderItems)
            .AsQueryable();

        // Search by order number or cashier
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(o =>
                o.OrderNumber.Contains(search) ||
                (o.CashierName != null && o.CashierName.Contains(search)));
        }

        // Date filters
        if (DateTime.TryParse(dateFrom, out var from))
            query = query.Where(o => o.OrderDate >= from);

        if (DateTime.TryParse(dateTo, out var to))
            query = query.Where(o => o.OrderDate <= to.AddDays(1));

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var orders = await query
            .OrderByDescending(o => o.OrderDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Summary stats for the filtered result
        var allFiltered = await query.ToListAsync();
        ViewBag.TotalOrders    = totalCount;
        ViewBag.TotalRevenue   = allFiltered.Sum(o => o.SubTotal);
        ViewBag.TodayOrders    = allFiltered.Count(o => o.OrderDate.Date == DateTime.UtcNow.Date);
        ViewBag.TodayRevenue   = allFiltered.Where(o => o.OrderDate.Date == DateTime.UtcNow.Date).Sum(o => o.SubTotal);

        ViewBag.CurrentPage    = page;
        ViewBag.TotalPages     = totalPages;
        ViewBag.Search         = search;
        ViewBag.DateFrom       = dateFrom;
        ViewBag.DateTo         = dateTo;

        return View(orders);
    }

    // GET: Orders/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();

        return View(order);
    }
}
