using MassTransit;

using UT.MicroserviceEco.DeliveryService.Domain;
using UT.MicroserviceEco.IntegrationContracts;

using Microsoft.EntityFrameworkCore;

using UT.MicroserviceEco.DeliveryService.Infrastructure;
using UT.MicroserviceEco.DeliveryService.Telemetry;

namespace UT.MicroserviceEco.DeliveryService.Consumers;

internal sealed class OrderCreatedConsumer(
    IServiceScopeFactory scopeFactory,
    ILogger<OrderCreatedConsumer> logger,
    DeliveryMetrics metrics) : IConsumer<OrderCreatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<OrderCreatedIntegrationEvent> context)
    {
        var message = context.Message;

        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DeliveryDbContext>();

        var exists = await dbContext.Deliveries.AsNoTracking()
            .AnyAsync(d => d.OrderId == message.OrderId);
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
        await dbContext.SaveChangesAsync();

        metrics.CreatedFromMessage();
        logger.LogInformation("Delivery row created for order {OrderId}", message.OrderId);
    }
}
