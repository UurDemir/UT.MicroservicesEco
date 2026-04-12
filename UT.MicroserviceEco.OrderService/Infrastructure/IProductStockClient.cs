namespace UT.MicroserviceEco.OrderService.Infrastructure;

internal interface IProductStockClient
{
    Task<bool> TryReserveAsync(Guid productId, int quantity, CancellationToken cancellationToken);

    Task ReleaseAsync(Guid productId, int quantity, CancellationToken cancellationToken);
}
