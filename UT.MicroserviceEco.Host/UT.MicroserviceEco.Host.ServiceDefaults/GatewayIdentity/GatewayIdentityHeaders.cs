namespace UT.MicroserviceEco.Host.ServiceDefaults.GatewayIdentity;

/// <summary>Headers set by ApiGateway after JWT validation for downstream services.</summary>
public static class GatewayIdentityHeaders
{
    public const string UserName = "X-Gateway-User-Name";
    public const string UserEmail = "X-Gateway-User-Email";
    public const string UserRoles = "X-Gateway-User-Roles";
}
