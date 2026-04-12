namespace UT.MicroserviceEco.ApiGateway.Configuration;

internal sealed class JwtOptions
{
    public string Key { get; init; } = string.Empty;
    public string Issuer { get; init; } = "UT.MicroserviceEco.AuthService";
    public string Audience { get; init; } = "UT.MicroserviceEco.ApiGateway";
}
