using System.Net.Http.Json;
using System.Text.Json;

namespace UT.MicroserviceEco.ProductService.Sdk;

public sealed class ProductServiceClient(HttpClient http) : IProductServiceClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<ProductInfo?> GetProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var response = await http.GetAsync($"api/products/{productId}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var dto = await response.Content.ReadFromJsonAsync<ProductResponseDto>(JsonOptions, cancellationToken);
        return dto is null ? null : new ProductInfo(dto.Id, dto.Name, dto.Price, dto.Stock);
    }

    public async Task<bool> TryReserveStockAsync(
        Guid productId,
        int quantity,
        GatewayCallerIdentity callerIdentity = default,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"api/products/{productId}/reserve");
        request.Content = JsonContent.Create(new { quantity });
        ApplyGatewayIdentity(request, callerIdentity);
        var response = await http.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task ReleaseStockAsync(
        Guid productId,
        int quantity,
        GatewayCallerIdentity callerIdentity = default,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"api/products/{productId}/release");
        request.Content = JsonContent.Create(new { quantity });
        ApplyGatewayIdentity(request, callerIdentity);
        await http.SendAsync(request, cancellationToken);
    }

    private static void ApplyGatewayIdentity(HttpRequestMessage request, GatewayCallerIdentity identity)
    {
        if (string.IsNullOrWhiteSpace(identity.UserName))
        {
            return;
        }

        request.Headers.TryAddWithoutValidation(GatewayIdentityHeaderNames.UserName, identity.UserName.Trim());
        if (!string.IsNullOrWhiteSpace(identity.UserEmail))
        {
            request.Headers.TryAddWithoutValidation(GatewayIdentityHeaderNames.UserEmail, identity.UserEmail.Trim());
        }

        if (!string.IsNullOrWhiteSpace(identity.UserRoles))
        {
            request.Headers.TryAddWithoutValidation(GatewayIdentityHeaderNames.UserRoles, identity.UserRoles.Trim());
        }
    }

    private sealed record ProductResponseDto(Guid Id, string Name, decimal Price, int Stock);
}
