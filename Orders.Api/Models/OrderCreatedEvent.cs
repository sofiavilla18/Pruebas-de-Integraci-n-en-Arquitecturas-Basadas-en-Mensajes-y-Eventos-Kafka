namespace Orders.Api.Models;

public class OrderCreatedEvent
{
    public string EventId { get; set; } = Guid.NewGuid().ToString();

    public string EventType { get; set; } = "OrderCreated";

    public string EventVersion { get; set; } = "1.0";

    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

    public string Source { get; set; } = "orders-api";

    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();

    public OrderData Data { get; set; } = new();
}

public class OrderData
{
    public string OrderId { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;

    public string Product { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal Total { get; set; }
}