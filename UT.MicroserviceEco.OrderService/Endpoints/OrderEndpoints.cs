using MassTransit;

using UT.MicroserviceEco.IntegrationContracts;
using UT.MicroserviceEco.OrderService.Domain;

using Microsoft.EntityFrameworkCore;

using UT.MicroserviceEco.OrderService.Contracts;
using UT.MicroserviceEco.OrderService.Infrastructure;
using UT.MicroserviceEco.OrderService.Telemetry;

namespace UT.MicroserviceEco.OrderService.Endpoints;

internal static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var orders = app.MapGroup("/api/orders");

        orders.MapGet("/", GetAllAsync);
        orders.MapGet("/{id:guid}", GetByIdAsync);
        orders.MapPost("/", CreateAsync).RequireAuthorization();
        orders.MapPut("/{id:guid}/status", UpdateStatusAsync).RequireAuthorization();

        return app;
    }

    private static async Task<IResult> GetAllAsync(OrderDbContext dbContext)
    {
        var items = await dbContext.Orders.AsNoTracking()
            .Include(o => o.Lines)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync();
        return Results.Ok(items);
    }

    private static async Task<IResult> GetByIdAsync(Guid id, OrderDbContext dbContext)
    {
        var item = await dbContext.Orders.AsNoTracking()
            .Include(o => o.Lines)
            .SingleOrDefaultAsync(x => x.Id == id);
        return item is null ? Results.NotFound() : Results.Ok(item);
    }

    private static async Task<IResult> CreateAsync(
        CreateOrderRequest request,
        OrderDbContext dbContext,
        IPublishEndpoint publishEndpoint,
        IProductStockClient stockClient,
        ILogger<Program> logger,
        OrderMetrics metrics,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserName)
            || string.IsNullOrWhiteSpace(request.ShippingAddress)
            || request.Lines.Count == 0)
        {
            return Results.BadRequest(new { message = "Invalid order payload." });
        }

        foreach (var line in request.Lines)
        {
            if (line.Quantity <= 0 || line.UnitPrice < 0 || line.ProductId == Guid.Empty)
            {
                return Results.BadRequest(new { message = "Invalid line item." });
            }
        }

        var total = request.Lines.Sum(l => l.Quantity * l.UnitPrice);
        var reserved = new List<(Guid ProductId, int Quantity)>();

        foreach (var line in request.Lines)
        {
            var ok = await stockClient.TryReserveAsync(line.ProductId, line.Quantity, cancellationToken);
            if (!ok)
            {
                foreach (var (productId, qty) in reserved.AsEnumerable().Reverse())
                {
                    await stockClient.ReleaseAsync(productId, qty, cancellationToken);
                }

                metrics.StockReservationFailed();
                logger.LogWarning(
                    "Order creation aborted: stock reservation failed for product {ProductId}",
                    line.ProductId);
                return Results.Conflict(new { message = "Stock reservation failed for one or more products." });
            }

            reserved.Add((line.ProductId, line.Quantity));
        }

        var order = new Order
        {
            UserName = request.UserName.Trim(),
            ShippingAddress = request.ShippingAddress.Trim(),
            TotalAmount = total,
            Status = "Pending"
        };

        foreach (var line in request.Lines)
        {
            order.Lines.Add(new OrderLine
            {
                ProductId = line.ProductId,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice
            });
        }

        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync(cancellationToken);

        var lineItems = request.Lines
            .Select(l => new OrderLineItem(l.ProductId, l.Quantity, l.UnitPrice))
            .ToList();

        await publishEndpoint.Publish(new OrderCreatedIntegrationEvent(
            order.Id,
            order.UserName,
            order.TotalAmount,
            order.ShippingAddress,
            lineItems), cancellationToken);

        metrics.OrderCreated();
        logger.LogInformation(
            "Order created {OrderId} for {UserName} total {TotalAmount}",
            order.Id,
            order.UserName,
            order.TotalAmount);

        return Results.Created($"/api/orders/{order.Id}", order);
    }

    private static async Task<IResult> UpdateStatusAsync(
        Guid id,
        UpdateOrderStatusRequest request,
        OrderDbContext dbContext,
        ILogger<Program> logger,
        OrderMetrics metrics)
    {
        var item = await dbContext.Orders.SingleOrDefaultAsync(x => x.Id == id);
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
        logger.LogInformation("Order {OrderId} status set to {Status}", id, item.Status);
        return Results.Ok(item);
    }
}
