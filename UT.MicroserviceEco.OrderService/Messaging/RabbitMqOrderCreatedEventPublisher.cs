using System.Text.Json;

using RabbitMQ.Client;

using UT.MicroserviceEco.IntegrationContracts;

namespace UT.MicroserviceEco.OrderService.Messaging;

internal sealed class RabbitMqOrderCreatedEventPublisher(IConnection connection) : IOrderCreatedEventPublisher
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public Task PublishAsync(OrderCreatedIntegrationEvent message, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var channel = connection.CreateModel();
        channel.ExchangeDeclare(OrderMessagingTopology.ExchangeName, ExchangeType.Topic, durable: true);

        var body = JsonSerializer.SerializeToUtf8Bytes(message, JsonOptions);
        var props = channel.CreateBasicProperties();
        props.Persistent = true;
        props.ContentType = "application/json";

        channel.BasicPublish(
            exchange: OrderMessagingTopology.ExchangeName,
            routingKey: OrderMessagingTopology.RoutingKey,
            basicProperties: props,
            body: body);

        return Task.CompletedTask;
    }
}
