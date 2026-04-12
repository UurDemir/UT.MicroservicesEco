namespace UT.MicroserviceEco.IntegrationContracts;

/// <summary>Shared RabbitMQ names for order integration (RabbitMQ.Client; no MassTransit).</summary>
public static class OrderMessagingTopology
{
    public const string ExchangeName = "ecommerce.orders";
    public const string RoutingKey = "order.created";
    public const string DeliveryQueueName = "delivery.order-created";
}
