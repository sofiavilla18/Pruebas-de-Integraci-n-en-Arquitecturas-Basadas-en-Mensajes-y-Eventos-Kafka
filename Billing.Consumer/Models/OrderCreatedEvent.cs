namespace Billing.Consumer.Models;

public class OrderCreatedEvent
{
    public string EventId { get; set; } = string.Empty;

    public string EventType { get; set; } = string.Empty;

    public string EventVersion { get; set; } = string.Empty;

    public DateTime OccurredAt { get; set; }

    public string Source { get; set; } = string.Empty;

    public string CorrelationId { get; set; } = string.Empty;

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