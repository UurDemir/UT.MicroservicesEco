namespace UT.MicroserviceEco.ProductService.Sdk;

/// <summary>Product snapshot returned from the catalog API.</summary>
public sealed record ProductInfo(Guid Id, string Name, decimal Price, int Stock);
