namespace UT.MicroserviceEco.OrderService.Domain;

internal sealed class Order
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string UserName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;

    public List<OrderLine> Lines { get; set; } = [];
}
