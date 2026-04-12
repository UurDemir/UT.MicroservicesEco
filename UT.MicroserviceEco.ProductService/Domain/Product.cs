namespace UT.MicroserviceEco.ProductService.Domain;

internal sealed class Product
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
}
