using Confluent.Kafka;
using System.Text.Json;
using Notification.Consumer.Models;

namespace Notification.Consumer;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;

    public Worker(
        ILogger<Worker> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _configuration["KAFKA_BROKER"]
                               ?? "localhost:29092",

            GroupId = _configuration["KAFKA_GROUP_ID"]
                      ?? "notification-service-group",

            AutoOffsetReset = AutoOffsetReset.Earliest,

            EnableAutoCommit = false
        };

        using var consumer = new ConsumerBuilder<string, string>(config)
            .Build();

        consumer.Subscribe("orders.events");

        _logger.LogInformation(
            "Notification Consumer iniciado. Escuchando orders.events...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(stoppingToken);

                var orderEvent =
                    JsonSerializer.Deserialize<OrderCreatedEvent>(
                        result.Message.Value);

                if (orderEvent == null ||
                    orderEvent.EventType != "OrderCreated")
                {
                    _logger.LogWarning("Evento no válido o no reconocido.");
                    continue;
                }

                _logger.LogInformation(
                    "Evento recibido: {EventId}", orderEvent.EventId);

                _logger.LogInformation(
                    "Pedido: {OrderId}", orderEvent.Data.OrderId);

                _logger.LogInformation(
                    "Cliente destinatario: {CustomerName}",
                    orderEvent.Data.CustomerName);

                _logger.LogInformation(
                    "Producto solicitado: {Product}",
                    orderEvent.Data.Product);

                _logger.LogInformation(
                    "Topic: {Topic} | Partition: {Partition} | Offset: {Offset}",
                    result.Topic,
                    result.Partition.Value,
                    result.Offset.Value);

                // Simular envío de notificación
                await Task.Delay(1000, stoppingToken);

                _logger.LogInformation(
                    "Notificación enviada correctamente al cliente {CustomerName} para el pedido {OrderId}",
                    orderEvent.Data.CustomerName,
                    orderEvent.Data.OrderId);

                // Confirmar el procesamiento
                consumer.Commit(result);

                _logger.LogInformation(
                    "Offset confirmado correctamente.");
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(
                    ex,
                    "Error al consumir mensaje de Kafka.");
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (JsonException ex)
            {
                _logger.LogError(
                    ex,
                    "Error al deserializar el evento.");
            }
        }

        consumer.Close();

        _logger.LogInformation(
            "Notification Consumer detenido.");
    }
}
