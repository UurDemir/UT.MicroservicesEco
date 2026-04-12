namespace UT.MicroserviceEco.Host.AppHost;

internal static class AspireProjectExtensions
{
    /// <summary>
    /// Sends OpenTelemetry traces and metrics to the AppHost OpenTelemetry Collector (OTLP gRPC).
    /// Waits for the collector to be available before starting the project.
    /// </summary>
    public static IResourceBuilder<ProjectResource> WithOtlpExporter(
        this IResourceBuilder<ProjectResource> builder,
        IResourceBuilder<ContainerResource> otelCollector) =>
        builder
            .WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", otelCollector.GetEndpoint("otlp-grpc"))
            .WithEnvironment("OTEL_EXPORTER_OTLP_PROTOCOL", "grpc")
            .WaitFor(otelCollector);
}
