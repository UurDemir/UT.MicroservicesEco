using Microsoft.AspNetCore.Diagnostics.HealthChecks;

using UT.MicroserviceEco.ApiGateway.Configuration;
using UT.MicroserviceEco.ApiGateway.Endpoints;
using UT.MicroserviceEco.ApiGateway.Security;
using UT.MicroserviceEco.ApiGateway.Telemetry;
using UT.MicroserviceEco.Host.ServiceDefaults;
using UT.MicroserviceEco.Host.ServiceDefaults.Telemetry;

using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();

var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
if (string.IsNullOrWhiteSpace(jwtOptions.Key) || jwtOptions.Key.Length < 32)
{
    throw new InvalidOperationException("Jwt:Key must be provided via user-secrets or environment variables and be at least 32 characters.");
}

builder.Services.AddGatewayAuthentication(jwtOptions);
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Authenticated", policy => policy.RequireAuthenticatedUser());
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver()
    .AddTransforms(builderContext =>
    {
        builderContext.AddRequestTransform(GatewayForwardIdentityTransform.ApplyAsync);
    });

builder.Services.AddECommerceMeter();
builder.Services.AddSingleton<GatewayMetrics>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(
                "https://ecommerce.ugurdemir.dev",
                "http://localhost:4200",
                "http://127.0.0.1:4200")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCorrelationId();
app.UseDefaultSerilogRequestLogging();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapGatewayEndpoints();
app.MapHealthChecks("/health", new HealthCheckOptions());
app.MapDefaultEndpoints();
app.Run();
