using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

using UT.MicroserviceEco.Host.ServiceDefaults.Telemetry;

namespace UT.MicroserviceEco.Host.ServiceDefaults;

// Adds common Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
// This project should be referenced by each service project in your solution.
// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
public static class Extensions
{
    private const string HealthEndpointPath = "/health";
    private const string AlivenessEndpointPath = "/alive";

    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        ApplyUnsecuredElasticsearchConnectionOverride(builder);

        builder.AddSerilogWithElasticsearch();

        // Aspire Elasticsearch integration: health checks, OTel tracing, and ElasticsearchClient DI when orchestrated.
        var elasticsearchConnection = ((IConfiguration)builder.Configuration).GetConnectionString("elasticsearch");
        if (!string.IsNullOrWhiteSpace(elasticsearchConnection))
        {
            builder.AddElasticsearchClient("elasticsearch");
        }

        builder.ConfigureOpenTelemetry();

        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            http.AddStandardResilienceHandler();

            // Turn on service discovery by default
            http.AddServiceDiscovery();
        });

        // Uncomment the following to restrict the allowed schemes for service discovery.
        // builder.Services.Configure<ServiceDiscoveryOptions>(options =>
        // {
        //     options.AllowedSchemes = ["https"];
        // });

        return builder;
    }

    /// <summary>
    /// With xpack.security disabled, credentials in the Aspire-generated connection string break the ES client health check.
    /// Override to a plain http://elasticsearch:9200-style URI when stripping is enabled (same default as Serilog).
    /// </summary>
    private static void ApplyUnsecuredElasticsearchConnectionOverride(IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("elasticsearch");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var stripAuth = builder.Configuration.GetValue("Serilog:Elasticsearch:StripAuthenticationCredentials", true);
        if (!stripAuth)
        {
            return;
        }

        var normalized = ElasticsearchConnectionFormatter.ToUnauthenticatedHttpUri(connectionString);
        if (builder.Configuration is IConfigurationManager configurationManager)
        {
            configurationManager.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:elasticsearch"] = normalized
            });
        }
    }

    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        var openTelemetry = builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddMeter(ECommerceMeter.Name)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddSource(builder.Environment.ApplicationName)
                    .AddAspNetCoreInstrumentation(tracing =>
                        // Exclude health check requests from tracing
                        tracing.Filter = context =>
                            !context.Request.Path.StartsWithSegments(HealthEndpointPath)
                            && !context.Request.Path.StartsWithSegments(AlivenessEndpointPath)
                    )
                    // Uncomment the following line to enable gRPC instrumentation (requires the OpenTelemetry.Instrumentation.GrpcNetClient package)
                    //.AddGrpcClientInstrumentation()
                    .AddHttpClientInstrumentation();
            });

        if (useOtlpExporter)
        {
            openTelemetry.UseOtlpExporter();
        }

        return builder;
    }

    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHealthChecks()
            // Add a default liveness check to ensure app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // Adding health checks endpoints to applications in non-development environments has security implications.
        // See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.
       
            // All health checks must pass for app to be considered ready to accept traffic after starting
            app.MapHealthChecks(HealthEndpointPath);

            // Only health checks tagged with the "live" tag must pass for app to be considered alive
            app.MapHealthChecks(AlivenessEndpointPath, new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live")
           });
        

        return app;
    }
}
