using Elastic.Clients.Elasticsearch.Requests;

using Microsoft.EntityFrameworkCore;

using UT.MicroserviceEco.BasketService.Contracts;
using UT.MicroserviceEco.BasketService.Domain;
using UT.MicroserviceEco.BasketService.Infrastructure;
using UT.MicroserviceEco.BasketService.Telemetry;
using UT.MicroserviceEco.ProductService.Sdk;

namespace UT.MicroserviceEco.BasketService.Endpoints;

internal static class BasketEndpoints
{
    public static IEndpointRouteBuilder MapBasketEndpoints(this IEndpointRouteBuilder app)
    {
        var baskets = app.MapGroup("/api/baskets");

        baskets.MapGet("/{userName}", GetByUserAsync);
        baskets.MapPost("/{userName}/items", AddItemAsync).RequireAuthorization();
        baskets.MapPut("/items/{id:guid}", UpdateItemAsync).RequireAuthorization();
        baskets.MapDelete("/items/{id:guid}", DeleteItemAsync).RequireAuthorization();

        return app;
    }

    private static async Task<IResult> GetByUserAsync(string userName, BasketDbContext dbContext)
    {
        var items = await dbContext.BasketItems.AsNoTracking()
            .Where(x => x.UserName == userName)
            .OrderByDescending(x => x.AddedAtUtc)
            .ToListAsync();
        return Results.Ok(items);
    }

    private static async Task<IResult> AddItemAsync(
        string userName,
        AddBasketItemRequest request,
        BasketDbContext dbContext,
        IProductServiceClient productService,
        ILogger<Program> logger,
        BasketMetrics metrics,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userName) || request.ProductId == Guid.Empty || request.Quantity <= 0)
        {
            return Results.BadRequest(new { message = "Invalid basket payload." });
        }

        var product = await productService.GetProductAsync(request.ProductId, cancellationToken);
        if (product is null)
        {
            logger.LogWarning("Add to basket failed: product {ProductId} not found", request.ProductId);
            return Results.NotFound(new { message = "Product not found." });
        }

        if (product.Stock < request.Quantity)
        {
            logger.LogWarning(
                "Add to basket failed: insufficient stock for product {ProductId} requested {Quantity}",
                request.ProductId,
                request.Quantity);
            return Results.Conflict(new { message = "Insufficient stock for this product." });
        }

        var item = new BasketItem
        {
            UserName = userName.Trim(),
            ProductId = product.Id,
            ProductName = product.Name,
            Quantity = request.Quantity,
            UnitPrice = product.Price
        };

        dbContext.BasketItems.Add(item);
        await dbContext.SaveChangesAsync(cancellationToken);
        metrics.ItemAdded(request.Quantity);
        logger.LogInformation(
            "Basket item added for {UserName} product {ProductId} line {ItemId}",
            item.UserName,
            item.ProductId,
            item.Id);
        return Results.Created($"/api/baskets/items/{item.Id}", item);
    }

    private static async Task<IResult> UpdateItemAsync(
        Guid id,
        UpdateBasketItemRequest request,
        BasketDbContext dbContext,
        IProductServiceClient productService,
        ILogger<Program> logger,
        BasketMetrics metrics,
        CancellationToken cancellationToken)
    {
        var item = await dbContext.BasketItems.SingleOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return Results.NotFound();
        }

        if (request.Quantity <= 0)
        {
            return Results.BadRequest(new { message = "Quantity must be greater than zero." });
        }

        var product = await productService.GetProductAsync(item.ProductId, cancellationToken);
        if (product is null)
        {
            logger.LogWarning("Basket update failed: product {ProductId} missing", item.ProductId);
            return Results.NotFound(new { message = "Product no longer exists." });
        }

        if (product.Stock < request.Quantity)
        {
            return Results.Conflict(new { message = "Insufficient stock for this product." });
        }

        item.Quantity = request.Quantity;
        item.UnitPrice = product.Price;
        item.ProductName = product.Name;
        await dbContext.SaveChangesAsync(cancellationToken);
        metrics.ItemUpdated(request.Quantity);
        logger.LogInformation("Basket item updated {ItemId} qty {Quantity}", id, request.Quantity);
        return Results.Ok(item);
    }

    private static async Task<IResult> DeleteItemAsync(
        Guid id,
        BasketDbContext dbContext,
        ILogger<Program> logger,
        BasketMetrics metrics)
    {
        var item = await dbContext.BasketItems.SingleOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return Results.NotFound();
        }
        
        var quantity = item.Quantity;
        dbContext.BasketItems.Remove(item);
        await dbContext.SaveChangesAsync();
        metrics.ItemRemoved(quantity);
        logger.LogInformation("Basket item removed {ItemId}", id);
        return Results.NoContent();
    }
}
