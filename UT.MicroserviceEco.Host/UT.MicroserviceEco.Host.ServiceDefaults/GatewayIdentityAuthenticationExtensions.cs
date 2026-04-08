using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Text;

namespace UT.MicroserviceEco.Host.ServiceDefaults;

public static class GatewayIdentityAuthenticationExtensions
{
    public static IServiceCollection AddTrustedGatewayIdentity(this IServiceCollection services, Action<AuthorizationOptions>? configurationAuthorization = null)
    {
        services.AddAuthorization(options =>
        {
            configurationAuthorization?.Invoke(options);
        });

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = "GatewayIdentity";
            options.DefaultChallengeScheme = "GatewayIdentity";
        });
        //Gateway için scheme


        return services;
    }
}
