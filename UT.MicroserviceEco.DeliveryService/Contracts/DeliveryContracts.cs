namespace UT.MicroserviceEco.DeliveryService.Contracts;

internal sealed record CreateDeliveryRequest(Guid OrderId, string Address);

internal sealed record UpdateDeliveryStatusRequest(string Status);
