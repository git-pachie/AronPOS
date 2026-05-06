using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PosApi.Data;
using PosApi.DTOs;

namespace PosApi.Controllers;

/// <summary>
/// Manages menu categories and items.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MenuController : ControllerBase
{
    private readonly AppDbContext _context;

    public MenuController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all active menu categories (without items).
    /// </summary>
    /// <returns>List of active categories.</returns>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(List<MenuCategoryDto>), 200)]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _context.MenuCategories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new MenuCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive
            })
            .ToListAsync();

        return Ok(categories);
    }

    /// <summary>
    /// Get all active categories including their available menu items.
    /// Used by the Android POS app on startup.
    /// </summary>
    /// <returns>List of categories each containing their available items.</returns>
    [HttpGet("categories/with-items")]
    [ProducesResponseType(typeof(List<MenuCategoryDto>), 200)]
    public async Task<IActionResult> GetCategoriesWithItems()
    {
        var categories = await _context.MenuCategories
            .Where(c => c.IsActive)
            .Include(c => c.MenuItems.Where(m => m.IsAvailable))
            .OrderBy(c => c.Name)
            .ToListAsync();

        var result = categories.Select(c => new MenuCategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            IsActive = c.IsActive,
            Items = c.MenuItems.Select(m => new MenuItemDto
            {
                Id             = m.Id,
                Name           = m.Name,
                Description    = m.Description,
                Price          = m.Price,
                IsAvailable    = m.IsAvailable,
                MenuCategoryId = m.MenuCategoryId,
                CategoryName   = c.Name,
                ImagePath      = m.ImagePath
            }).ToList()
        }).ToList();

        return Ok(result);
    }

    /// <summary>
    /// Get available menu items, optionally filtered by category.
    /// </summary>
    /// <param name="categoryId">Optional category ID to filter items.</param>
    /// <returns>List of available menu items.</returns>
    [HttpGet("items")]
    [ProducesResponseType(typeof(List<MenuItemDto>), 200)]
    public async Task<IActionResult> GetItems([FromQuery] int? categoryId)
    {
        var query = _context.MenuItems
            .Include(m => m.MenuCategory)
            .Where(m => m.IsAvailable)
            .AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(m => m.MenuCategoryId == categoryId.Value);

        var items = await query
            .OrderBy(m => m.Name)
            .Select(m => new MenuItemDto
            {
                Id             = m.Id,
                Name           = m.Name,
                Description    = m.Description,
                Price          = m.Price,
                IsAvailable    = m.IsAvailable,
                MenuCategoryId = m.MenuCategoryId,
                CategoryName   = m.MenuCategory.Name,
                ImagePath      = m.ImagePath
            })
            .ToListAsync();

        return Ok(items);
    }

    /// <summary>
    /// Get a single menu item by ID.
    /// </summary>
    /// <param name="id">The menu item ID.</param>
    /// <returns>The menu item details.</returns>
    [HttpGet("items/{id}")]
    [ProducesResponseType(typeof(MenuItemDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetItem(int id)
    {
        var item = await _context.MenuItems
            .Include(m => m.MenuCategory)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (item == null) return NotFound();

        return Ok(new MenuItemDto
        {
            Id             = item.Id,
            Name           = item.Name,
            Description    = item.Description,
            Price          = item.Price,
            IsAvailable    = item.IsAvailable,
            MenuCategoryId = item.MenuCategoryId,
            CategoryName   = item.MenuCategory.Name,
            ImagePath      = item.ImagePath
        });
    }
}
