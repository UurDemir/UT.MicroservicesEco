using UT.MicroserviceEco.ProductService.Sdk;
using UT.MicroserviceEco.Host.ServiceDefaults;
using UT.MicroserviceEco.Host.ServiceDefaults.Telemetry;

using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

using UT.MicroserviceEco.BasketService.Endpoints;
using UT.MicroserviceEco.BasketService.Infrastructure;
using UT.MicroserviceEco.BasketService.Telemetry;
using UT.MicroserviceEco.Host.ServiceDefaults.GatewayIdentity;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("basketdb")
    ?? throw new InvalidOperationException("Connection string 'basketdb' is required.");
builder.Services.AddDbContext<BasketDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddTrustedGatewayIdentity();
builder.Services.AddProductServiceClient(new Uri("http://productservice"));
builder.Services.AddECommerceMeter();
builder.Services.AddSingleton<BasketMetrics>();

var app = builder.Build();
await BasketDbSeeder.SeedAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCorrelationId();
app.UseDefaultSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapBasketEndpoints();
app.MapHealthChecks("/health", new HealthCheckOptions());
app.MapDefaultEndpoints();
app.Run();
