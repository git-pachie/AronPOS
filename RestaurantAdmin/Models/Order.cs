using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantAdmin.Models;

public class Order
{
    public int Id { get; set; }

    [Required]
    public string OrderNumber { get; set; } = string.Empty;

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "decimal(10,2)")]
    public decimal SubTotal { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal CashGiven { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Change { get; set; }

    public string Status { get; set; } = "Completed";

    public string? CashierName { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
