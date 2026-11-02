namespace Orders.Api.Models;

public class CreateOrderRequest
{
    public string CustomerName { get; set; } = string.Empty;

    public string Product { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }
}