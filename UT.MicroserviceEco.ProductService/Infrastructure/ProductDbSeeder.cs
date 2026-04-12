using UT.MicroserviceEco.ProductService.Domain;

using Microsoft.EntityFrameworkCore;

namespace UT.MicroserviceEco.ProductService.Infrastructure;

internal static class ProductDbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();

        await dbContext.Database.EnsureCreatedAsync();

        if (!await dbContext.Products.AnyAsync())
        {
            dbContext.Products.AddRange(
                new Product { Name = "Sample Keyboard", Price = 49.90m, Stock = 20 },
                new Product { Name = "Sample Mouse", Price = 19.90m, Stock = 50 });
            await dbContext.SaveChangesAsync();
        }
    }
}
