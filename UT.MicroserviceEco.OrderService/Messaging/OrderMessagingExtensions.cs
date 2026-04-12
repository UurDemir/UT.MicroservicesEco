using MassTransit;

namespace UT.MicroserviceEco.OrderService.Messaging;

internal static class OrderMessagingExtensions
{
    public static IServiceCollection AddOrderMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitConnection = configuration.GetConnectionString("rabbitmq")
            ?? throw new InvalidOperationException("Connection string 'rabbitmq' is required (Aspire RabbitMQ reference).");

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            x.UsingRabbitMq((_, cfg) =>
            {
                cfg.Host(new Uri(rabbitConnection));
            });
        });

        return services;
    }
}
