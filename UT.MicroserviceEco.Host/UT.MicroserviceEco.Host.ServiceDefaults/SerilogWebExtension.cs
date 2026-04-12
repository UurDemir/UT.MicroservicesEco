using Microsoft.AspNetCore.Builder;

using Serilog;

namespace UT.MicroserviceEco.Host.ServiceDefaults;

public static class SerilogWebExtensions
{
    public static WebApplication UseDefaultSerilogRequestLogging(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("CorrelationId", httpContext.TraceIdentifier);
                diagnosticContext.Set("RequestPath", httpContext.Request.Path);
            };
        });

        return app;
    }
}