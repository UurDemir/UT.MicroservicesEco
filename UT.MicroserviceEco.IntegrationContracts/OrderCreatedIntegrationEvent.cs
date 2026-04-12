namespace UT.MicroserviceEco.IntegrationContracts;

/// <summary>
/// Published when an order is persisted. Consumed by DeliveryService to create a shipment record.
/// </summary>
public sealed record OrderCreatedIntegrationEvent(
    Guid OrderId,
    string UserName,
    decimal TotalAmount,
    string ShippingAddress,
    IReadOnlyList<OrderLineItem> LineItems);
