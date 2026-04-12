using UT.MicroserviceEco.DeliveryService.Domain;

using Microsoft.EntityFrameworkCore;

using UT.MicroserviceEco.DeliveryService.Contracts;
using UT.MicroserviceEco.DeliveryService.Infrastructure;
using UT.MicroserviceEco.DeliveryService.Telemetry;

namespace UT.MicroserviceEco.DeliveryService.Endpoints;

internal static class DeliveryEndpoints
{
    public static IEndpointRouteBuilder MapDeliveryEndpoints(this IEndpointRouteBuilder app)
    {
        var deliveries = app.MapGroup("/api/deliveries");

        deliveries.MapGet("/", GetAllAsync);
        deliveries.MapGet("/order/{orderId:guid}", GetByOrderIdAsync);
        deliveries.MapPost("/", CreateAsync).RequireAuthorization();
        deliveries.MapPut("/{id:guid}/status", UpdateStatusAsync).RequireAuthorization();

        return app;
    }

    private static async Task<IResult> GetAllAsync(DeliveryDbContext dbContext)
    {
        var items = await dbContext.Deliveries.AsNoTracking().OrderByDescending(x => x.CreatedAtUtc).ToListAsync();
        return Results.Ok(items);
    }

    private static async Task<IResult> GetByOrderIdAsync(Guid orderId, DeliveryDbContext dbContext)
    {
        var item = await dbContext.Deliveries.AsNoTracking().SingleOrDefaultAsync(x => x.OrderId == orderId);
        return item is null ? Results.NotFound() : Results.Ok(item);
    }

    private static async Task<IResult> CreateAsync(
        CreateDeliveryRequest request,
        DeliveryDbContext dbContext,
        ILogger<Program> logger,
        DeliveryMetrics metrics)
    {
        if (request.OrderId == Guid.Empty || string.IsNullOrWhiteSpace(request.Address))
        {
            return Results.BadRequest(new { message = "Invalid delivery payload." });
        }

        var delivery = new Delivery
        {
            OrderId = request.OrderId,
            Address = request.Address.Trim(),
            Status = "Preparing"
        };
        dbContext.Deliveries.Add(delivery);
        await dbContext.SaveChangesAsync();
        metrics.CreatedFromApi();
        logger.LogInformation("Delivery created {DeliveryId} for order {OrderId}", delivery.Id, delivery.OrderId);
        return Results.Created($"/api/deliveries/{delivery.Id}", delivery);
    }

    private static async Task<IResult> UpdateStatusAsync(
        Guid id,
        UpdateDeliveryStatusRequest request,
        DeliveryDbContext dbContext,
        ILogger<Program> logger,
        DeliveryMetrics metrics)
    {
        var item = await dbContext.Deliveries.SingleOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return Results.NotFound();
        }

        if (string.IsNullOrWhiteSpace(request.Status))
        {
            return Results.BadRequest(new { message = "Status is required." });
        }

        item.Status = request.Status.Trim();
        await dbContext.SaveChangesAsync();
        metrics.StatusUpdated();
        logger.LogInformation("Delivery {DeliveryId} status set to {Status}", id, item.Status);
        return Results.Ok(item);
    }
}
