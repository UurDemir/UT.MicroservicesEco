namespace UT.MicroserviceEco.ProductService.Sdk;

/// <summary>Typed client for ProductService HTTP APIs. Prefer this over ad-hoc HttpClient usage in microservices.</summary>
public interface IProductServiceClient
{
    Task<ProductInfo?> GetProductAsync(Guid productId, CancellationToken cancellationToken = default);

    Task<bool> TryReserveStockAsync(
        Guid productId,
        int quantity,
        GatewayCallerIdentity callerIdentity = default,
        CancellationToken cancellationToken = default);

    Task ReleaseStockAsync(
        Guid productId,
        int quantity,
        GatewayCallerIdentity callerIdentity = default,
        CancellationToken cancellationToken = default);
}
