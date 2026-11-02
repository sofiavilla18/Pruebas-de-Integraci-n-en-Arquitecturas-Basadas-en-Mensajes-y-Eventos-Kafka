using Confluent.Kafka;
using Orders.Api.Models;

var builder = WebApplication.CreateBuilder(args);

// Configuración de Kafka
var kafkaBroker = Environment.GetEnvironmentVariable("KAFKA_BROKER")
                  ?? "localhost:29092";

var kafkaTopic = Environment.GetEnvironmentVariable("KAFKA_TOPIC")
                 ?? "orders.events";

// Configurar el productor de Kafka
var producerConfig = new ProducerConfig
{
    BootstrapServers = kafkaBroker,
    Acks = Acks.All,
    EnableIdempotence = true
};

builder.Services.AddSingleton<IProducer<string, string>>(
    new ProducerBuilder<string, string>(producerConfig).Build()
);

var app = builder.Build();

app.MapGet("/", () => "Orders API funcionando correctamente");

// Endpoint para crear pedidos
app.MapPost("/orders", async (
    CreateOrderRequest request,
    IProducer<string, string> producer) =>
{
    // Validar los datos recibidos
    if (string.IsNullOrWhiteSpace(request.CustomerName) ||
        string.IsNullOrWhiteSpace(request.Product) ||
        request.Quantity <= 0 ||
        request.UnitPrice <= 0)
    {
        return Results.BadRequest(new
        {
            status = "error",
            message = "Los datos del pedido no son válidos."
        });
    }

    // Generar identificador del pedido
    var orderId = $"ORD-{Guid.NewGuid():N}";

    // Construir el evento
    var orderEvent = new OrderCreatedEvent
    {
        EventId = Guid.NewGuid().ToString(),
        EventType = "OrderCreated",
        EventVersion = "1.0",
        OccurredAt = DateTime.UtcNow,
        Source = "orders-api",
        CorrelationId = Guid.NewGuid().ToString(),
        Data = new OrderData
        {
            OrderId = orderId,
            CustomerName = request.CustomerName,
            Product = request.Product,
            Quantity = request.Quantity,
            UnitPrice = request.UnitPrice,
            Total = request.Quantity * request.UnitPrice
        }
    };

    // Serializar el evento a JSON
    var eventJson = System.Text.Json.JsonSerializer.Serialize(orderEvent);

    try
    {
        // Publicar el evento en Kafka
        var deliveryResult = await producer.ProduceAsync(
            kafkaTopic,
            new Message<string, string>
            {
                Key = orderId,
                Value = eventJson
            });

        Console.WriteLine(
            $"Evento publicado. Topic: {deliveryResult.Topic}, " +
            $"Partition: {deliveryResult.Partition}, " +
            $"Offset: {deliveryResult.Offset}");

        return Results.Accepted($"/orders/{orderId}", new
        {
            status = "accepted",
            message = "Pedido recibido y evento OrderCreated publicado en Kafka",
            orderId
        });
    }
    catch (ProduceException<string, string> ex)
    {
        Console.WriteLine($"Error publicando en Kafka: {ex.Error.Reason}");

        return Results.Problem(
            title: "No fue posible publicar el evento en Kafka",
            detail: ex.Error.Reason,
            statusCode: 503);
    }
});

app.Run();