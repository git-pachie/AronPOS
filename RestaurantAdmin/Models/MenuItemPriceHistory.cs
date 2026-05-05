using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantAdmin.Models;

public class MenuItemPriceHistory
{
    public int Id { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal OldPrice { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal NewPrice { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public string ChangedByUserId { get; set; } = string.Empty;

    public int MenuItemId { get; set; }
    public MenuItem MenuItem { get; set; } = null!;
}
