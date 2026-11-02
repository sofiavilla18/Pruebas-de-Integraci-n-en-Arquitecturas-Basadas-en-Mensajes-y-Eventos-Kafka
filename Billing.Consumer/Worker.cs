using Confluent.Kafka;
using System.Text.Json;
using Billing.Consumer.Models;

namespace Billing.Consumer;

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
                      ?? "billing-service-group",

            AutoOffsetReset = AutoOffsetReset.Earliest,

            EnableAutoCommit = false
        };

        using var consumer = new ConsumerBuilder<string, string>(config)
            .Build();

        consumer.Subscribe("orders.events");

        _logger.LogInformation(
            "Billing Consumer iniciado. Escuchando orders.events...");

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
                    "Cliente: {CustomerName}",
                    orderEvent.Data.CustomerName);

                _logger.LogInformation(
                    "Producto: {Product}", orderEvent.Data.Product);

                _logger.LogInformation(
                    "Total de la factura: {Total}",
                    orderEvent.Data.Total);

                _logger.LogInformation(
                    "Topic: {Topic} | Partition: {Partition} | Offset: {Offset}",
                    result.Topic,
                    result.Partition.Value,
                    result.Offset.Value);

                // Simular generación de factura
                await Task.Delay(1500, stoppingToken);

                _logger.LogInformation(
                    "Factura generada correctamente para {OrderId}",
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
            "Billing Consumer detenido.");
    }
}