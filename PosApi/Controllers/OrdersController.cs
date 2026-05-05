using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PosApi.Data;
using PosApi.DTOs;
using PosApi.Models;

namespace PosApi.Controllers;

/// <summary>
/// Handles order creation and retrieval.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _context;

    public OrdersController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Submit a new order. Validates item availability and cash amount.
    /// Returns the order with calculated change.
    /// </summary>
    /// <param name="request">Order details including items and cash given.</param>
    /// <returns>The created order with order number and change.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(OrderResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        if (request.Items == null || !request.Items.Any())
            return BadRequest(new { message = "Order must have at least one item." });

        var itemIds = request.Items.Select(i => i.MenuItemId).ToList();
        var menuItems = await _context.MenuItems
            .Where(m => itemIds.Contains(m.Id) && m.IsAvailable)
            .ToListAsync();

        if (menuItems.Count != itemIds.Distinct().Count())
            return BadRequest(new { message = "One or more items are unavailable." });

        var orderItems = request.Items.Select(ri =>
        {
            var menuItem = menuItems.First(m => m.Id == ri.MenuItemId);
            return new OrderItem
            {
                MenuItemId = ri.MenuItemId,
                ItemName   = menuItem.Name,
                UnitPrice  = menuItem.Price,
                Quantity   = ri.Quantity,
                LineTotal  = menuItem.Price * ri.Quantity
            };
        }).ToList();

        var subTotal = orderItems.Sum(i => i.LineTotal);
        var change   = request.CashGiven - subTotal;

        if (change < 0)
            return BadRequest(new { message = "Cash given is less than the total amount." });

        var order = new Order
        {
            OrderNumber  = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}",
            SubTotal     = subTotal,
            CashGiven    = request.CashGiven,
            Change       = change,
            CashierName  = request.CashierName,
            OrderItems   = orderItems
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return Ok(new OrderResponse
        {
            Id          = order.Id,
            OrderNumber = order.OrderNumber,
            OrderDate   = order.OrderDate,
            SubTotal    = order.SubTotal,
            CashGiven   = order.CashGiven,
            Change      = order.Change,
            Status      = order.Status,
            Items       = orderItems.Select(i => new OrderItemResponse
            {
                ItemName  = i.ItemName,
                UnitPrice = i.UnitPrice,
                Quantity  = i.Quantity,
                LineTotal = i.LineTotal
            }).ToList()
        });
    }

    /// <summary>
    /// Get paginated order history, newest first.
    /// </summary>
    /// <param name="page">Page number (default 1).</param>
    /// <param name="pageSize">Items per page (default 20).</param>
    /// <returns>List of orders.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<OrderResponse>), 200)]
    public async Task<IActionResult> GetOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var orders = await _context.Orders
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.OrderDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new OrderResponse
            {
                Id          = o.Id,
                OrderNumber = o.OrderNumber,
                OrderDate   = o.OrderDate,
                SubTotal    = o.SubTotal,
                CashGiven   = o.CashGiven,
                Change      = o.Change,
                Status      = o.Status,
                Items       = o.OrderItems.Select(i => new OrderItemResponse
                {
                    ItemName  = i.ItemName,
                    UnitPrice = i.UnitPrice,
                    Quantity  = i.Quantity,
                    LineTotal = i.LineTotal
                }).ToList()
            })
            .ToListAsync();

        return Ok(orders);
    }

    /// <summary>
    /// Get a single order by ID.
    /// </summary>
    /// <param name="id">The order ID.</param>
    /// <returns>The order details.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetOrder(int id)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();

        return Ok(new OrderResponse
        {
            Id          = order.Id,
            OrderNumber = order.OrderNumber,
            OrderDate   = order.OrderDate,
            SubTotal    = order.SubTotal,
            CashGiven   = order.CashGiven,
            Change      = order.Change,
            Status      = order.Status,
            Items       = order.OrderItems.Select(i => new OrderItemResponse
            {
                ItemName  = i.ItemName,
                UnitPrice = i.UnitPrice,
                Quantity  = i.Quantity,
                LineTotal = i.LineTotal
            }).ToList()
        });
    }
}
