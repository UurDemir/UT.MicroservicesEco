using Microsoft.AspNetCore.Identity.Data;

using UT.MicroserviceEco.AuthService.Application;
using UT.MicroserviceEco.AuthService.Infrastructure;
using UT.MicroserviceEco.AuthService.Telemetry;
using Microsoft.EntityFrameworkCore;

namespace UT.MicroserviceEco.AuthService.Endpoints;

internal static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/api/auth");

        auth.MapPost("/register", RegisterAsync);
        auth.MapPost("/login", LoginAsync);

        return app;
    }


    private static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        AuthDbContext dbContext,
        ILogger<Program> logger,
        AuthMetrics metrics)
    {
        if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Results.BadRequest(new { message = "Username and password are required." });
        }

        var normalizedUserName = request.UserName.Trim();
        var alreadyExists = await dbContext.Users.AnyAsync(u => u.UserName == normalizedUserName);
        if (alreadyExists)
        {
            logger.LogWarning("Registration rejected: user {UserName} already exists", normalizedUserName);
            return Results.Conflict(new { message = "User already exists." });
        }

        var user = AppUser.Create(normalizedUserName, request.Email, request.Password, ["Customer"]);
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        metrics.RegistrationCompleted();
        logger.LogInformation("User registered: {UserName}", user.UserName);

        return Results.Created($"/api/auth/users/{user.UserName}", new
        {
            user.UserName,
            user.Email,
            roles = user.GetRoles()
        });
    }


    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        AuthDbContext dbContext,
        JwtTokenService tokenService,
        ILogger<Program> logger,
        AuthMetrics metrics)
    {
        metrics.LoginAttempt();
        var userName = request.UserName?.Trim() ?? string.Empty;
        var user = await dbContext.Users.SingleOrDefaultAsync(u => u.UserName == userName);
        if (user is null || !PasswordHasher.Verify(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            metrics.LoginFailed();
            logger.LogWarning("Login failed for user {UserName}", userName);
            return Results.Unauthorized();
        }

        var response = tokenService.CreateToken(user);
        logger.LogInformation("User logged in: {UserName}", user.UserName);
        return Results.Ok(response);
    }

}
