namespace UT.MicroserviceEco.IntegrationContracts;

/// <summary>
/// Published when an order is persisted. Consumed by DeliveryService to create a shipment record.
/// </summary>
/// <param name="LineItems">Array for reliable JSON (de)serialization with System.Text.Json.</param>
public sealed record OrderCreatedIntegrationEvent(
    Guid OrderId,
    string UserName,
    decimal TotalAmount,
    string ShippingAddress,
    OrderLineItem[] LineItems);
