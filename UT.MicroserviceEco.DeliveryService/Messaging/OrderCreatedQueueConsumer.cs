using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using UT.MicroserviceEco.DeliveryService.Domain;
using UT.MicroserviceEco.DeliveryService.Infrastructure;
using UT.MicroserviceEco.DeliveryService.Telemetry;
using UT.MicroserviceEco.IntegrationContracts;

namespace UT.MicroserviceEco.DeliveryService.Messaging;

internal sealed class OrderCreatedQueueConsumer(
    IConnection connection,
    IServiceScopeFactory scopeFactory,
    ILogger<OrderCreatedQueueConsumer> logger,
    DeliveryMetrics metrics) : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly SemaphoreSlim _channelSerial = new(1, 1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = connection.CreateModel();
        try
        {
            channel.ExchangeDeclare(OrderMessagingTopology.ExchangeName, ExchangeType.Topic, durable: true);
            channel.QueueDeclare(OrderMessagingTopology.DeliveryQueueName, durable: true, exclusive: false, autoDelete: false);
            channel.QueueBind(OrderMessagingTopology.DeliveryQueueName, OrderMessagingTopology.ExchangeName, OrderMessagingTopology.RoutingKey);
            channel.BasicQos(0, prefetchCount: 10, global: false);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += (_, ea) => OnReceivedAsync(channel, ea);

            channel.BasicConsume(OrderMessagingTopology.DeliveryQueueName, autoAck: false, consumer);

            try
            {
                await Task.Delay(Timeout.Infinite, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // shutdown
            }
        }
        finally
        {
            _channelSerial.Dispose();
            try
            {
                channel.Close();
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex, "RabbitMQ channel close");
            }

            channel.Dispose();
        }
    }

    private async Task OnReceivedAsync(IModel channel, BasicDeliverEventArgs ea)
    {
        await _channelSerial.WaitAsync().ConfigureAwait(false);
        try
        {
            var message = JsonSerializer.Deserialize<OrderCreatedIntegrationEvent>(ea.Body.Span, JsonOptions);
            if (message is null)
            {
                channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
                return;
            }

            await HandleMessageAsync(message).ConfigureAwait(false);
            channel.BasicAck(ea.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed processing order-created message; requeueing");
            channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
        }
        finally
        {
            _channelSerial.Release();
        }
    }

    private async Task HandleMessageAsync(OrderCreatedIntegrationEvent message)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DeliveryDbContext>();

        var exists = await dbContext.Deliveries.AsNoTracking()
            .AnyAsync(d => d.OrderId == message.OrderId).ConfigureAwait(false);
        if (exists)
        {
            metrics.DuplicateOrderSkipped();
            logger.LogDebug("Delivery already exists for order {OrderId}, skipping", message.OrderId);
            return;
        }

        dbContext.Deliveries.Add(new Delivery
        {
            OrderId = message.OrderId,
            Address = message.ShippingAddress,
            Status = "Preparing"
        });
        await dbContext.SaveChangesAsync().ConfigureAwait(false);

        metrics.CreatedFromMessage();
        logger.LogInformation("Delivery row created for order {OrderId}", message.OrderId);
    }
}
