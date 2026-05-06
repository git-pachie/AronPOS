using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAdmin.Data;
using RestaurantAdmin.Helpers;
using RestaurantAdmin.Models;
using RestaurantAdmin.ViewModels;

namespace RestaurantAdmin.Controllers;

[Authorize(Roles = "Admin,Manager")]
public class MenuController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _env;

    public MenuController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment env)
    {
        _context = context;
        _userManager = userManager;
        _env = env;
    }

    // ─── Categories ──────────────────────────────────────────────────────────

    public async Task<IActionResult> Categories()
    {
        var categories = await _context.MenuCategories
            .Include(c => c.MenuItems)
            .OrderBy(c => c.Name)
            .Select(c => new MenuCategoryViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive,
                ItemCount = c.MenuItems.Count
            })
            .ToListAsync();

        return View(categories);
    }

    public IActionResult CreateCategory() => View(new MenuCategoryViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(MenuCategoryViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        _context.MenuCategories.Add(new MenuCategory
        {
            Name = model.Name,
            Description = model.Description,
            IsActive = model.IsActive
        });

        await _context.SaveChangesAsync();
        TempData["Success"] = $"Category '{model.Name}' created.";
        return RedirectToAction(nameof(Categories));
    }

    public async Task<IActionResult> EditCategory(int id)
    {
        var category = await _context.MenuCategories.FindAsync(id);
        if (category == null) return NotFound();

        return View(new MenuCategoryViewModel
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCategory(MenuCategoryViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var category = await _context.MenuCategories.FindAsync(model.Id);
        if (category == null) return NotFound();

        category.Name = model.Name;
        category.Description = model.Description;
        category.IsActive = model.IsActive;

        await _context.SaveChangesAsync();
        TempData["Success"] = $"Category '{category.Name}' updated.";
        return RedirectToAction(nameof(Categories));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.MenuCategories
            .Include(c => c.MenuItems)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null) return NotFound();

        if (category.MenuItems.Any())
        {
            TempData["Error"] = "Cannot delete a category that has menu items. Remove items first.";
            return RedirectToAction(nameof(Categories));
        }

        _context.MenuCategories.Remove(category);
        await _context.SaveChangesAsync();
        TempData["Success"] = $"Category '{category.Name}' deleted.";
        return RedirectToAction(nameof(Categories));
    }

    // ─── Menu Items ───────────────────────────────────────────────────────────

    public async Task<IActionResult> Items(int? categoryId)
    {
        var query = _context.MenuItems
            .Include(m => m.MenuCategory)
            .AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(m => m.MenuCategoryId == categoryId.Value);

        var items = await query
            .OrderBy(m => m.MenuCategory.Name)
            .ThenBy(m => m.Name)
            .Select(m => new MenuItemViewModel
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

        ViewBag.Categories = await _context.MenuCategories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();

        ViewBag.SelectedCategoryId = categoryId;
        return View(items);
    }

    // GET: Menu/CreateItem
    public async Task<IActionResult> CreateItem()
    {
        return View(new MenuItemViewModel
        {
            Categories = await GetCategoryViewModels()
        });
    }

    // POST: Menu/CreateItem
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(1L * 1024 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 1L * 1024 * 1024 * 1024)]
    public async Task<IActionResult> CreateItem(MenuItemViewModel model, IFormFile? imageFile)
    {
        ModelState.Remove("ImageFile");

        if (!ModelState.IsValid)
        {
            model.Categories = await GetCategoryViewModels();
            return View(model);
        }

        string? imagePath = null;

        if (imageFile != null && imageFile.Length > 0)
        {
            var saveResult = await SaveImageFile(imageFile);
            if (saveResult.Error != null)
            {
                ModelState.AddModelError("ImageFile", saveResult.Error);
                model.Categories = await GetCategoryViewModels();
                return View(model);
            }
            imagePath = saveResult.Path;
        }

        _context.MenuItems.Add(new MenuItem
        {
            Name           = model.Name,
            Description    = model.Description,
            Price          = model.Price,
            IsAvailable    = model.IsAvailable,
            MenuCategoryId = model.MenuCategoryId,
            ImagePath      = imagePath
        });

        await _context.SaveChangesAsync();
        TempData["Success"] = $"Menu item '{model.Name}' created.";
        return RedirectToAction(nameof(Items));
    }

    // GET: Menu/EditItem/id
    public async Task<IActionResult> EditItem(int id)
    {
        var item = await _context.MenuItems.FindAsync(id);
        if (item == null) return NotFound();

        return View(new MenuItemViewModel
        {
            Id             = item.Id,
            Name           = item.Name,
            Description    = item.Description,
            Price          = item.Price,
            IsAvailable    = item.IsAvailable,
            MenuCategoryId = item.MenuCategoryId,
            ImagePath      = item.ImagePath,
            Categories     = await GetCategoryViewModels()
        });
    }

    // POST: Menu/EditItem
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(1L * 1024 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 1L * 1024 * 1024 * 1024)]
    public async Task<IActionResult> EditItem(MenuItemViewModel model, IFormFile? imageFile)
    {
        ModelState.Remove("ImageFile");

        if (!ModelState.IsValid)
        {
            model.Categories = await GetCategoryViewModels();
            return View(model);
        }

        var item = await _context.MenuItems.FindAsync(model.Id);
        if (item == null) return NotFound();

        item.Name           = model.Name;
        item.Description    = model.Description;
        item.IsAvailable    = model.IsAvailable;
        item.MenuCategoryId = model.MenuCategoryId;
        item.UpdatedAt      = DateTime.UtcNow;

        if (imageFile != null && imageFile.Length > 0)
        {
            var saveResult = await SaveImageFile(imageFile, item.ImagePath);
            if (saveResult.Error != null)
            {
                ModelState.AddModelError("ImageFile", saveResult.Error);
                model.Categories = await GetCategoryViewModels();
                model.ImagePath  = item.ImagePath;
                return View(model);
            }
            item.ImagePath = saveResult.Path;
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = $"Menu item '{item.Name}' updated.";
        return RedirectToAction(nameof(Items));
    }

    // POST: Menu/RemoveItemImage/id
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveItemImage(int id)
    {
        var item = await _context.MenuItems.FindAsync(id);
        if (item == null) return NotFound();

        DeleteImageFile(item.ImagePath);
        item.ImagePath = null;
        item.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Image removed from '{item.Name}'.";
        return RedirectToAction(nameof(EditItem), new { id });
    }

    // GET: Menu/UpdatePrice/id
    public async Task<IActionResult> UpdatePrice(int id)
    {
        var item = await _context.MenuItems.FindAsync(id);
        if (item == null) return NotFound();

        return View(new UpdatePriceViewModel
        {
            MenuItemId   = item.Id,
            ItemName     = item.Name,
            CurrentPrice = item.Price,
            NewPrice     = item.Price
        });
    }

    // POST: Menu/UpdatePrice
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePrice(UpdatePriceViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var item = await _context.MenuItems.FindAsync(model.MenuItemId);
        if (item == null) return NotFound();

        if (item.Price == model.NewPrice)
        {
            ModelState.AddModelError("NewPrice", "New price must be different from the current price.");
            model.CurrentPrice = item.Price;
            model.ItemName     = item.Name;
            return View(model);
        }

        var currentUserId = _userManager.GetUserId(User) ?? string.Empty;

        _context.MenuItemPriceHistories.Add(new MenuItemPriceHistory
        {
            MenuItemId      = item.Id,
            OldPrice        = item.Price,
            NewPrice        = model.NewPrice,
            ChangedByUserId = currentUserId
        });

        item.Price     = model.NewPrice;
        item.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        TempData["Success"] = $"Price for '{item.Name}' updated to {model.NewPrice:C}.";
        return RedirectToAction(nameof(Items));
    }

    // GET: Menu/PriceHistory/id
    public async Task<IActionResult> PriceHistory(int id)
    {
        var item = await _context.MenuItems
            .Include(m => m.PriceHistory)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (item == null) return NotFound();

        var userIds = item.PriceHistory.Select(h => h.ChangedByUserId).Distinct().ToList();
        var users   = _userManager.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionary(u => u.Id, u => u.FullName);

        var vm = new PriceHistoryViewModel
        {
            ItemName     = item.Name,
            CurrentPrice = item.Price,
            History      = item.PriceHistory
                .OrderByDescending(h => h.ChangedAt)
                .Select(h => new PriceHistoryEntry
                {
                    OldPrice  = h.OldPrice,
                    NewPrice  = h.NewPrice,
                    ChangedAt = h.ChangedAt,
                    ChangedBy = users.TryGetValue(h.ChangedByUserId, out var name) ? name : "Unknown"
                })
                .ToList()
        };

        return View(vm);
    }

    // POST: Menu/DeleteItem/id
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteItem(int id)
    {
        var item = await _context.MenuItems.FindAsync(id);
        if (item == null) return NotFound();

        DeleteImageFile(item.ImagePath);
        _context.MenuItems.Remove(item);
        await _context.SaveChangesAsync();
        TempData["Success"] = $"Menu item '{item.Name}' deleted.";
        return RedirectToAction(nameof(Items));
    }

    // ─── Private helpers ──────────────────────────────────────────────────────

    private async Task<(string? Path, string? Error)> SaveImageFile(
        IFormFile file, string? oldPath = null)
    {
        var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowed.Contains(ext))
            return (null, "Only JPG, PNG, GIF or WEBP images are allowed.");

        // No file size limit enforced — up to 1 GB allowed by server config

        // Delete old image first
        DeleteImageFile(oldPath);

        // Ensure upload directory exists
        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "products");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"product_{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(uploadsDir, fileName);

        using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return ($"/uploads/products/{fileName}", null);
    }

    private void DeleteImageFile(string? relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) return;
        var full = Path.Combine(_env.WebRootPath,
            relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (System.IO.File.Exists(full))
            System.IO.File.Delete(full);
    }

    private async Task<List<MenuCategoryViewModel>> GetCategoryViewModels()
    {
        return await _context.MenuCategories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new MenuCategoryViewModel { Id = c.Id, Name = c.Name })
            .ToListAsync();
    }
}
