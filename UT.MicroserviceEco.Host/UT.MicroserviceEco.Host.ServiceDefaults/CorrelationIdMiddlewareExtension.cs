using Microsoft.AspNetCore.Builder;

using Serilog.Context;

namespace UT.MicroserviceEco.Host.ServiceDefaults;

public static class CorrelationIdMiddlewareExtension
{
    public const string CorrelationIdHeaderName = "X-Correlation-ID";
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        return app.Use(async (context,next) =>
        {
            var correlationId = context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var headerValue) && !string.IsNullOrEmpty(headerValue) 
            ? headerValue.ToString() 
            : Guid.NewGuid().ToString("N");

            context.TraceIdentifier = correlationId;
            context.Response.Headers[CorrelationIdHeaderName] = correlationId;

            using (LogContext.PushProperty("CorrelationId",correlationId))
            {
                await next();
            }
        });
    }
}
