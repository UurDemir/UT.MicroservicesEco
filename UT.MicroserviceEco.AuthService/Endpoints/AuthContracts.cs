namespace UT.MicroserviceEco.AuthService.Endpoints;

internal sealed record RegisterRequest(string UserName, string Email, string Password);

internal sealed record LoginRequest(string UserName, string Password);

internal sealed record LoginResponse(string AccessToken, DateTime ExpiresAtUtc);
