using System.Diagnostics.Metrics;

namespace UT.MicroserviceEco.ProductService.Telemetry;

internal sealed class ProductMetrics
{
    private readonly Counter<long> _created;
    private readonly Counter<long> _updated;
    private readonly Counter<long> _deleted;
    private readonly Counter<long> _stockReserved;
    private readonly Counter<long> _stockReserveConflicts;
    private readonly Counter<long> _stockReleased;

    public ProductMetrics(Meter meter)
    {
        _created = meter.CreateCounter<long>("ecommerce.products.created", description: "Products created");
        _updated = meter.CreateCounter<long>("ecommerce.products.updated", description: "Products updated");
        _deleted = meter.CreateCounter<long>("ecommerce.products.deleted", description: "Products deleted");
        _stockReserved = meter.CreateCounter<long>("ecommerce.products.stock.reserved", description: "Successful stock reservations");
        _stockReserveConflicts = meter.CreateCounter<long>(
            "ecommerce.products.stock.reserve_conflicts",
            description: "Stock reservations that failed (insufficient stock)");
        _stockReleased = meter.CreateCounter<long>("ecommerce.products.stock.released", description: "Stock releases");
    }

    public void ProductCreated() => _created.Add(1);

    public void ProductUpdated() => _updated.Add(1);

    public void ProductDeleted() => _deleted.Add(1);

    public void StockReserved() => _stockReserved.Add(1);

    public void StockReserveConflict() => _stockReserveConflicts.Add(1);

    public void StockReleased() => _stockReleased.Add(1);
}
