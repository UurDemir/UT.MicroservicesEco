using UT.MicroserviceEco.Host.ServiceDefaults;
using UT.MicroserviceEco.Host.ServiceDefaults.Telemetry;

using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

using UT.MicroserviceEco.ProductService.Endpoints;
using UT.MicroserviceEco.ProductService.Infrastructure;
using UT.MicroserviceEco.ProductService.Telemetry;
using UT.MicroserviceEco.Host.ServiceDefaults.GatewayIdentity;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("productdb")
    ?? throw new InvalidOperationException("Connection string 'productdb' is required.");
builder.Services.AddDbContext<ProductDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddTrustedGatewayIdentity();
builder.Services.AddECommerceMeter();
builder.Services.AddSingleton<ProductMetrics>();

var app = builder.Build();
await ProductDbSeeder.SeedAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCorrelationId();
app.UseDefaultSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapProductEndpoints();
app.MapHealthChecks("/health", new HealthCheckOptions());
app.MapDefaultEndpoints();
app.Run();
