using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceExample.ServiceDefaults.Telemetry;

public static class MetricsServiceCollectionExtensions
{
    /// <summary>Registers a singleton <see cref="Meter"/> for domain counters/histograms (OTel must call <c>AddMeter(ECommerceMeter.Name)</c>).</summary>
    public static IServiceCollection AddECommerceMeter(this IServiceCollection services)
    {
        services.AddSingleton(_ => new Meter(ECommerceMeter.Name, ECommerceMeter.Version));
        return services;
    }
}
