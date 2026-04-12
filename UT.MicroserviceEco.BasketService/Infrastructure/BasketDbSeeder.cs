namespace UT.MicroserviceEco.BasketService.Infrastructure;

internal static class BasketDbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BasketDbContext>();

        await dbContext.Database.EnsureCreatedAsync();
    }
}
