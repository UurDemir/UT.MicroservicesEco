using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace UT.MicroserviceEco.Host.ServiceDefaults.GatewayIdentity;

public static class GatewayIdentityAuthenticationExtensions
{
    /// <summary>
    /// Registers authentication that trusts <see cref="GatewayIdentityHeaders"/> set by ApiGateway.
    /// Downstream services do not validate JWTs; they rely on network isolation and the gateway as the security boundary.
    /// </summary>
    public static IServiceCollection AddTrustedGatewayIdentity(
        this IServiceCollection services,
        Action<AuthorizationOptions>? configureAuthorization = null)
    {
        services.AddAuthorization(options =>
        {
            configureAuthorization?.Invoke(options);
        });

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = GatewayIdentityDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = GatewayIdentityDefaults.AuthenticationScheme;
            })
            .AddScheme<AuthenticationSchemeOptions, GatewayIdentityAuthenticationHandler>(
                GatewayIdentityDefaults.AuthenticationScheme,
                _ => { });

        return services;
    }
}
