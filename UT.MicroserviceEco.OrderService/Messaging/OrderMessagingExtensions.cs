using RabbitMQ.Client;

namespace UT.MicroserviceEco.OrderService.Messaging;

internal static class OrderMessagingExtensions
{
    public static IServiceCollection AddOrderMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitConnection = configuration.GetConnectionString("rabbitmq")
            ?? throw new InvalidOperationException("Connection string 'rabbitmq' is required (Aspire RabbitMQ reference).");

        services.AddSingleton<IConnection>(_ =>
        {
            var factory = new ConnectionFactory { Uri = new Uri(rabbitConnection) };
            return factory.CreateConnection();
        });

        services.AddSingleton<IOrderCreatedEventPublisher, RabbitMqOrderCreatedEventPublisher>();

        return services;
    }
}
