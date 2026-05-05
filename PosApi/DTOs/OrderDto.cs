namespace PosApi.DTOs;

public class CreateOrderRequest
{
    public decimal CashGiven { get; set; }
    public string? CashierName { get; set; }
    public List<OrderItemRequest> Items { get; set; } = new();
}

public class OrderItemRequest
{
    public int MenuItemId { get; set; }
    public int Quantity { get; set; }
}

public class OrderResponse
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal CashGiven { get; set; }
    public decimal Change { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<OrderItemResponse> Items { get; set; } = new();
}

public class OrderItemResponse
{
    public string ItemName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}
