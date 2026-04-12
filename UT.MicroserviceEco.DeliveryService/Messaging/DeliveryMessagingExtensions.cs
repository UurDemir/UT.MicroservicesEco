using RabbitMQ.Client;

namespace UT.MicroserviceEco.DeliveryService.Messaging;

internal static class DeliveryMessagingExtensions
{
    public static IServiceCollection AddDeliveryMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitConnection = configuration.GetConnectionString("rabbitmq")
            ?? throw new InvalidOperationException("Connection string 'rabbitmq' is required (Aspire RabbitMQ reference).");

        services.AddSingleton<IConnection>(_ =>
        {
            var factory = new ConnectionFactory { Uri = new Uri(rabbitConnection) };
            return factory.CreateConnection();
        });

        services.AddHostedService<OrderCreatedQueueConsumer>();

        return services;
    }
}
