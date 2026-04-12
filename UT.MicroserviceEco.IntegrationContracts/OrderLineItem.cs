namespace UT.MicroserviceEco.IntegrationContracts;

public sealed record OrderLineItem(Guid ProductId, int Quantity, decimal UnitPrice);
