using UT.MicroserviceEco.IntegrationContracts;

namespace UT.MicroserviceEco.OrderService.Messaging;

internal interface IOrderCreatedEventPublisher
{
    Task PublishAsync(OrderCreatedIntegrationEvent message, CancellationToken cancellationToken);
}
