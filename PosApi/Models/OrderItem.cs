using System.ComponentModel.DataAnnotations.Schema;

namespace PosApi.Models;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int MenuItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;

    [Column(TypeName = "decimal(10,2)")]
    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal LineTotal { get; set; }
}
