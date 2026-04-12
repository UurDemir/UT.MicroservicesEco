using Microsoft.AspNetCore.Diagnostics.HealthChecks;

using UT.MicroserviceEco.ApiGateway.Telemetry;

namespace UT.MicroserviceEco.ApiGateway.Endpoints;

internal static class GatewayEndpoints
{
    public static IEndpointRouteBuilder MapGatewayEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", (ILogger<Program> logger, GatewayMetrics metrics) =>
        {
            metrics.RootRequested();
            logger.LogInformation("ApiGateway status probe");
            return Results.Ok(new { service = "ApiGateway", status = "ok" });
        });
        app.MapHealthChecks("/health", new HealthCheckOptions());
        app.MapReverseProxy();
        return app;
    }
}
