namespace UT.MicroserviceEco.BasketService.Domain;

internal sealed class BasketItem
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string UserName { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public DateTime AddedAtUtc { get; init; } = DateTime.UtcNow;
}
