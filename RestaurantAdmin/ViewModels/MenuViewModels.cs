using System.ComponentModel.DataAnnotations;

namespace RestaurantAdmin.ViewModels;

public class MenuCategoryViewModel
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    [Display(Name = "Category Name")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    public int ItemCount { get; set; }
}

public class MenuItemViewModel
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    [Display(Name = "Item Name")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    [Range(0.01, 99999.99, ErrorMessage = "Price must be greater than 0.")]
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }

    [Display(Name = "Available")]
    public bool IsAvailable { get; set; } = true;

    [Required]
    [Display(Name = "Category")]
    public int MenuCategoryId { get; set; }

    public string? CategoryName { get; set; }
    public string? ImagePath { get; set; }

    public List<MenuCategoryViewModel> Categories { get; set; } = new();
}

public class UpdatePriceViewModel
{
    public int MenuItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }

    [Required]
    [Range(0.01, 99999.99, ErrorMessage = "Price must be greater than 0.")]
    [Display(Name = "New Price")]
    [DataType(DataType.Currency)]
    public decimal NewPrice { get; set; }
}

public class PriceHistoryViewModel
{
    public string ItemName { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public List<PriceHistoryEntry> History { get; set; } = new();
}

public class PriceHistoryEntry
{
    public decimal OldPrice { get; set; }
    public decimal NewPrice { get; set; }
    public DateTime ChangedAt { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
}
