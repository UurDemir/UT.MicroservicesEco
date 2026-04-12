using Microsoft.Extensions.DependencyInjection;

namespace UT.MicroserviceEco.ProductService.Sdk;

public static class ServiceCollectionExtensions
{
    /// <summary>Registers <see cref="IProductServiceClient"/> with a typed <see cref="HttpClient"/>.</summary>
    /// <param name="baseAddress">Base URL of ProductService (e.g. <c>http://productservice/</c>).</param>
    public static IServiceCollection AddProductServiceClient(this IServiceCollection services, Uri baseAddress)
    {
        services.AddHttpClient<IProductServiceClient, ProductServiceClient>(client =>
        {
            client.BaseAddress = baseAddress;
        });
        return services;
    }
}
