using System.Diagnostics.Metrics;

namespace UT.MicroserviceEco.OrderService.Telemetry;

internal sealed class OrderMetrics
{
    private readonly Counter<long> _created;
    private readonly Counter<long> _stockReservationFailures;
    private readonly Counter<long> _statusUpdates;

    public OrderMetrics(Meter meter)
    {
        _created = meter.CreateCounter<long>("ecommerce.orders.created", description: "Orders persisted after successful stock hold");
        _stockReservationFailures = meter.CreateCounter<long>(
            "ecommerce.orders.stock_reservation_failures",
            description: "Order creations aborted due to stock reservation failure");
        _statusUpdates = meter.CreateCounter<long>("ecommerce.orders.status_updates", description: "Order status changes");
    }

    public void OrderCreated() => _created.Add(1);

    public void StockReservationFailed() => _stockReservationFailures.Add(1);

    public void StatusUpdated() => _statusUpdates.Add(1);
}
