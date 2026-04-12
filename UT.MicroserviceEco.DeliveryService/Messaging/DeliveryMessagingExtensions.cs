using MassTransit;

using UT.MicroserviceEco.DeliveryService.Consumers;

namespace UT.MicroserviceEco.DeliveryService.Messaging;

internal static class DeliveryMessagingExtensions
{
    public static IServiceCollection AddDeliveryMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitConnection = configuration.GetConnectionString("rabbitmq")
            ?? throw new InvalidOperationException("Connection string 'rabbitmq' is required (Aspire RabbitMQ reference).");

        services.AddMassTransit(x =>
        {
            x.AddConsumer<OrderCreatedConsumer>();
            x.SetKebabCaseEndpointNameFormatter();
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri(rabbitConnection));
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
