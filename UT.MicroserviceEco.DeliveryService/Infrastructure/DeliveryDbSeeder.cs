using UT.MicroserviceEco.DeliveryService.Domain;

using Microsoft.EntityFrameworkCore;

namespace UT.MicroserviceEco.DeliveryService.Infrastructure;

internal static class DeliveryDbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DeliveryDbContext>();

        await dbContext.Database.EnsureCreatedAsync();

        if (!await dbContext.Deliveries.AnyAsync())
        {
            dbContext.Deliveries.Add(new Delivery
            {
                OrderId = Guid.NewGuid(),
                Address = "Sample Address, Demo City",
                Status = "Preparing"
            });
            await dbContext.SaveChangesAsync();
        }
    }
}
