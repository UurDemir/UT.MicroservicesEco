using UT.MicroserviceEco.ProductService.Sdk;
using UT.MicroserviceEco.Host.ServiceDefaults;
using UT.MicroserviceEco.Host.ServiceDefaults.Telemetry;

using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

using UT.MicroserviceEco.OrderService.Endpoints;
using UT.MicroserviceEco.OrderService.Infrastructure;
using UT.MicroserviceEco.OrderService.Messaging;
using UT.MicroserviceEco.OrderService.Telemetry;
using UT.MicroserviceEco.Host.ServiceDefaults.GatewayIdentity;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("orderdb")
    ?? throw new InvalidOperationException("Connection string 'orderdb' is required.");
builder.Services.AddDbContext<OrderDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddTrustedGatewayIdentity();
builder.Services.AddHttpContextAccessor();
builder.Services.AddProductServiceClient(new Uri("http://productservice"));
builder.Services.AddScoped<IProductStockClient, ProductStockClient>();
builder.Services.AddOrderMessaging(builder.Configuration);
builder.Services.AddECommerceMeter();
builder.Services.AddSingleton<OrderMetrics>();

var app = builder.Build();
await OrderDbSeeder.SeedAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCorrelationId();
app.UseDefaultSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapOrderEndpoints();
app.MapHealthChecks("/health", new HealthCheckOptions());
app.MapDefaultEndpoints();
app.Run();
