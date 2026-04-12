namespace UT.MicroserviceEco.Host.ServiceDefaults.GatewayIdentity;

/// <summary>Authentication scheme used when identity is supplied by the API gateway (no JWT validation downstream).</summary>
public static class GatewayIdentityDefaults
{
    public const string AuthenticationScheme = "GatewayIdentity";
}
