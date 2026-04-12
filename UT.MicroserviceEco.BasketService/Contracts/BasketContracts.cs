namespace UT.MicroserviceEco.BasketService.Contracts;

internal sealed record AddBasketItemRequest(Guid ProductId, int Quantity);

internal sealed record UpdateBasketItemRequest(int Quantity);
