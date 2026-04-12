using System.Text.Json.Serialization;

namespace UT.MicroserviceEco.OrderService.Domain;

internal sealed class OrderLine
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    [JsonIgnore]
    public Order Order { get; set; } = null!;
}
