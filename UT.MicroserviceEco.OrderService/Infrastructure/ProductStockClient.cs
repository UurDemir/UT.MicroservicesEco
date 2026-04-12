using UT.MicroserviceEco.ProductService.Sdk;

namespace UT.MicroserviceEco.OrderService.Infrastructure;

/// <summary>Order-domain wrapper that forwards gateway identity headers to ProductService via the SDK.</summary>
internal sealed class ProductStockClient(IProductServiceClient productService, IHttpContextAccessor httpContextAccessor)
    : IProductStockClient
{
    public Task<bool> TryReserveAsync(Guid productId, int quantity, CancellationToken cancellationToken)
    {
        var identity = FromHttpContext(httpContextAccessor.HttpContext);
        return productService.TryReserveStockAsync(productId, quantity, identity, cancellationToken);
    }

    public Task ReleaseAsync(Guid productId, int quantity, CancellationToken cancellationToken)
    {
        var identity = FromHttpContext(httpContextAccessor.HttpContext);
        return productService.ReleaseStockAsync(productId, quantity, identity, cancellationToken);
    }

    private static GatewayCallerIdentity FromHttpContext(HttpContext? http)
    {
        if (http is null)
        {
            return default;
        }

        return new GatewayCallerIdentity(
            http.Request.Headers[GatewayIdentityHeaderNames.UserName].FirstOrDefault(),
            http.Request.Headers[GatewayIdentityHeaderNames.UserEmail].FirstOrDefault(),
            http.Request.Headers[GatewayIdentityHeaderNames.UserRoles].FirstOrDefault());
    }
}
