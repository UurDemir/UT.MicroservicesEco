
using Microsoft.EntityFrameworkCore;

namespace UT.MicroserviceEco.AuthService.Infrastructure;

internal static class AuthDbSeeder
{
    public static async Task SeedAdminAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        await dbContext.Database.EnsureCreatedAsync();

        if (!await dbContext.Users.AnyAsync(u => u.UserName == "admin"))
        {
            dbContext.Users.Add(AppUser.Create("admin", "admin@local", "Admin123!", ["Admin"]));
            await dbContext.SaveChangesAsync();
        }
    }
}
