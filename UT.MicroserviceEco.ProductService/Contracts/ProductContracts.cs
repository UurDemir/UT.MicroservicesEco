namespace UT.MicroserviceEco.ProductService.Contracts;

internal sealed record CreateProductRequest(string Name, decimal Price, int Stock);

internal sealed record UpdateProductRequest(string Name, decimal Price, int Stock);

internal sealed record AdjustStockRequest(int Quantity);
