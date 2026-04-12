using UT.MicroserviceEco.Host.ServiceDefaults;
using UT.MicroserviceEco.Host.ServiceDefaults.Telemetry;

using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

using UT.MicroserviceEco.DeliveryService.Endpoints;
using UT.MicroserviceEco.DeliveryService.Infrastructure;
using UT.MicroserviceEco.DeliveryService.Messaging;
using UT.MicroserviceEco.DeliveryService.Telemetry;
using UT.MicroserviceEco.Host.ServiceDefaults.GatewayIdentity;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("deliverydb")
    ?? throw new InvalidOperationException("Connection string 'deliverydb' is required.");
builder.Services.AddDbContext<DeliveryDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddTrustedGatewayIdentity();
builder.Services.AddDeliveryMessaging(builder.Configuration);
builder.Services.AddECommerceMeter();
builder.Services.AddSingleton<DeliveryMetrics>();

var app = builder.Build();
await DeliveryDbSeeder.SeedAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCorrelationId();
app.UseDefaultSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapDeliveryEndpoints();
app.MapHealthChecks("/health", new HealthCheckOptions());
app.MapDefaultEndpoints();
app.Run();
