using System.Diagnostics.Metrics;

namespace UT.MicroserviceEco.BasketService.Telemetry;

internal sealed class BasketMetrics
{
    private readonly Counter<long> _itemsAdded;
    private readonly Counter<long> _itemsUpdated;
    private readonly Counter<long> _itemsRemoved;

    public BasketMetrics(Meter meter)
    {
        _itemsAdded = meter.CreateCounter<long>("ecommerce.basket.items.added", description: "Basket line items added");
        _itemsUpdated = meter.CreateCounter<long>("ecommerce.basket.items.updated", description: "Basket line items updated");
        _itemsRemoved = meter.CreateCounter<long>("ecommerce.basket.items.removed", description: "Basket line items removed");
    }

    public void ItemAdded(int count) => _itemsAdded.Add(count);

    public void ItemUpdated(int count) => _itemsUpdated.Add(count);

    public void ItemRemoved(int count) => _itemsRemoved.Add(count);
}
