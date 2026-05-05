using System.ComponentModel.DataAnnotations;

namespace RestaurantAdmin.Models;

public class MenuCategory
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
}
