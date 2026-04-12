namespace UT.MicroserviceEco.OrderService.Contracts;

internal sealed record OrderLineRequest(Guid ProductId, int Quantity, decimal UnitPrice);

internal sealed record CreateOrderRequest(string UserName, string ShippingAddress, IReadOnlyList<OrderLineRequest> Lines);

internal sealed record UpdateOrderStatusRequest(string Status);
