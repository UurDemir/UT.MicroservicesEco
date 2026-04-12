namespace UT.MicroserviceEco.DeliveryService.Domain;

internal sealed class Delivery
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Status { get; set; } = "Preparing";
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
}
