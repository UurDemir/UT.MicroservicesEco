using System.Diagnostics.Metrics;

namespace UT.MicroserviceEco.DeliveryService.Telemetry;

internal sealed class DeliveryMetrics
{
    private readonly Counter<long> _createdFromApi;
    private readonly Counter<long> _createdFromMessages;
    private readonly Counter<long> _statusUpdates;
    private readonly Counter<long> _duplicateOrderSkips;

    public DeliveryMetrics(Meter meter)
    {
        _createdFromApi = meter.CreateCounter<long>("ecommerce.deliveries.created.api", description: "Deliveries created via HTTP API");
        _createdFromMessages = meter.CreateCounter<long>(
            "ecommerce.deliveries.created.messaging",
            description: "Deliveries created from order integration events");
        _statusUpdates = meter.CreateCounter<long>("ecommerce.deliveries.status_updates", description: "Delivery status updates");
        _duplicateOrderSkips = meter.CreateCounter<long>(
            "ecommerce.deliveries.duplicate_order_skipped",
            description: "Integration events ignored because delivery already existed");
    }

    public void CreatedFromApi() => _createdFromApi.Add(1);

    public void CreatedFromMessage() => _createdFromMessages.Add(1);

    public void StatusUpdated() => _statusUpdates.Add(1);

    public void DuplicateOrderSkipped() => _duplicateOrderSkips.Add(1);
}
