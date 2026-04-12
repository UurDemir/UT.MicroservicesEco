using UT.MicroserviceEco.ProductService.Domain;

using Microsoft.EntityFrameworkCore;

using UT.MicroserviceEco.ProductService.Contracts;
using UT.MicroserviceEco.ProductService.Infrastructure;
using UT.MicroserviceEco.ProductService.Telemetry;

namespace UT.MicroserviceEco.ProductService.Endpoints;

internal static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var products = app.MapGroup("/api/products");

        products.MapGet("/", GetAllAsync);
        products.MapGet("/{id:guid}", GetByIdAsync);
        products.MapPost("/", CreateAsync).RequireAuthorization();
        products.MapPut("/{id:guid}", UpdateAsync).RequireAuthorization();
        products.MapDelete("/{id:guid}", DeleteAsync).RequireAuthorization();
        products.MapPost("/{id:guid}/reserve", ReserveStockAsync).RequireAuthorization();
        products.MapPost("/{id:guid}/release", ReleaseStockAsync).RequireAuthorization();

        return app;
    }

    private static async Task<IResult> GetAllAsync(ProductDbContext dbContext)
    {
        var items = await dbContext.Products.AsNoTracking().OrderBy(x => x.Name).ToListAsync();
        return Results.Ok(items);
    }

    private static async Task<IResult> GetByIdAsync(Guid id, ProductDbContext dbContext)
    {
        var item = await dbContext.Products.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
        return item is null ? Results.NotFound() : Results.Ok(item);
    }

    private static async Task<IResult> CreateAsync(
        CreateProductRequest request,
        ProductDbContext dbContext,
        ILogger<Program> logger,
        ProductMetrics metrics)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.Price < 0 || request.Stock < 0)
        {
            return Results.BadRequest(new { message = "Invalid product payload." });
        }

        var item = new Product
        {
            Name = request.Name.Trim(),
            Price = request.Price,
            Stock = request.Stock
        };

        dbContext.Products.Add(item);
        await dbContext.SaveChangesAsync();
        metrics.ProductCreated();
        logger.LogInformation("Product created {ProductId} {Name}", item.Id, item.Name);
        return Results.Created($"/api/products/{item.Id}", item);
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateProductRequest request,
        ProductDbContext dbContext,
        ILogger<Program> logger,
        ProductMetrics metrics)
    {
        var item = await dbContext.Products.SingleOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return Results.NotFound();
        }

        if (string.IsNullOrWhiteSpace(request.Name) || request.Price < 0 || request.Stock < 0)
        {
            return Results.BadRequest(new { message = "Invalid product payload." });
        }

        item.Name = request.Name.Trim();
        item.Price = request.Price;
        item.Stock = request.Stock;
        await dbContext.SaveChangesAsync();
        metrics.ProductUpdated();
        logger.LogInformation("Product updated {ProductId}", id);
        return Results.Ok(item);
    }

    private static async Task<IResult> DeleteAsync(
        Guid id,
        ProductDbContext dbContext,
        ILogger<Program> logger,
        ProductMetrics metrics)
    {
        var item = await dbContext.Products.SingleOrDefaultAsync(x => x.Id == id);
        if (item is null)
        {
            return Results.NotFound();
        }

        dbContext.Products.Remove(item);
        await dbContext.SaveChangesAsync();
        metrics.ProductDeleted();
        logger.LogInformation("Product deleted {ProductId}", id);
        return Results.NoContent();
    }

    private static async Task<IResult> ReserveStockAsync(
        Guid id,
        AdjustStockRequest request,
        ProductDbContext dbContext,
        ILogger<Program> logger,
        ProductMetrics metrics)
    {
        if (request.Quantity <= 0)
        {
            return Results.BadRequest(new { message = "Quantity must be greater than zero." });
        }

        var quantity = request.Quantity;
        var updated = await dbContext.Products
            .Where(p => p.Id == id && p.Stock >= quantity)
            .ExecuteUpdateAsync(setters => setters.SetProperty(p => p.Stock, p => p.Stock - quantity));

        if (updated == 0)
        {
            var exists = await dbContext.Products.AsNoTracking().AnyAsync(p => p.Id == id);
            if (exists)
            {
                metrics.StockReserveConflict();
                logger.LogWarning("Stock reserve conflict for product {ProductId} qty {Quantity}", id, quantity);
            }

            return exists
                ? Results.Conflict(new { message = "Insufficient stock." })
                : Results.NotFound();
        }

        metrics.StockReserved();
        logger.LogInformation("Stock reserved for product {ProductId} qty {Quantity}", id, quantity);
        return Results.NoContent();
    }

    private static async Task<IResult> ReleaseStockAsync(
        Guid id,
        AdjustStockRequest request,
        ProductDbContext dbContext,
        ILogger<Program> logger,
        ProductMetrics metrics)
    {
        if (request.Quantity <= 0)
        {
            return Results.BadRequest(new { message = "Quantity must be greater than zero." });
        }

        var quantity = request.Quantity;
        var updated = await dbContext.Products
            .Where(p => p.Id == id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(p => p.Stock, p => p.Stock + quantity));

        if (updated == 0)
        {
            return Results.NotFound();
        }

        metrics.StockReleased();
        logger.LogInformation("Stock released for product {ProductId} qty {Quantity}", id, quantity);
        return Results.NoContent();
    }
}
