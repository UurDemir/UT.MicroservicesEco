using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

using UT.MicroserviceEco.AuthService.Application;
using UT.MicroserviceEco.AuthService.Endpoints;
using UT.MicroserviceEco.AuthService.Infrastructure;
using UT.MicroserviceEco.AuthService.Telemetry;
using UT.MicroserviceEco.Host.ServiceDefaults;
using UT.MicroserviceEco.Host.ServiceDefaults.GatewayIdentity;
using UT.MicroserviceEco.Host.ServiceDefaults.Telemetry;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("authdb") ?? throw new InvalidOperationException("Connection string 'authdb' is required.");
builder.Services.AddDbContext<AuthDbContext>(options => options.UseNpgsql(connectionString));

var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();

builder.Services.AddSingleton(jwtOptions);
builder.Services.AddSingleton<JwtTokenService>();


builder.Services.AddTrustedGatewayIdentity(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

builder.Services.AddECommerceMeter();
builder.Services.AddSingleton<AuthMetrics>();

var app = builder.Build();

await AuthDbSeeder.SeedAdminAsync(app.Services);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.UseCorrelationId();
app.UseDefaultSerilogRequestLogging();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();


app.MapAuthEndpoints();
app.MapHealthChecks("/health", new HealthCheckOptions());
app.MapDefaultEndpoints();
app.Run();