using System.Diagnostics.Metrics;

namespace UT.MicroserviceEco.ApiGateway.Telemetry;

internal sealed class GatewayMetrics
{
    private readonly Counter<long> _rootProbe;

    public GatewayMetrics(Meter meter)
    {
        _rootProbe = meter.CreateCounter<long>(
            "ecommerce.gateway.root_requests",
            description: "Requests to the gateway root health endpoint");
    }

    public void RootRequested() => _rootProbe.Add(1);
}
