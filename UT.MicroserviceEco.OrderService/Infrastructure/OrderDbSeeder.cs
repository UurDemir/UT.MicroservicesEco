namespace UT.MicroserviceEco.OrderService.Infrastructure;

internal static class OrderDbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

        await dbContext.Database.EnsureCreatedAsync();
    }
}
