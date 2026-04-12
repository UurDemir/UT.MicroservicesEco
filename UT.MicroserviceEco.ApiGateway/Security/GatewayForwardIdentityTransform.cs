using System.Security.Claims;

using UT.MicroserviceEco.Host.ServiceDefaults.GatewayIdentity;

using Yarp.ReverseProxy.Transforms;

namespace UT.MicroserviceEco.ApiGateway.Security;

internal static class GatewayForwardIdentityTransform
{
    public static ValueTask ApplyAsync(RequestTransformContext context)
    {
        context.ProxyRequest.Headers.Remove("Authorization");

        var user = context.HttpContext.User;
        if (user.Identity?.IsAuthenticated != true)
        {
            return ValueTask.CompletedTask;
        }

        var name = user.FindFirst(ClaimTypes.Name)?.Value
            ?? user.FindFirst("unique_name")?.Value;
        if (!string.IsNullOrWhiteSpace(name))
        {
            context.ProxyRequest.Headers.TryAddWithoutValidation(GatewayIdentityHeaders.UserName, name.Trim());
        }

        var email = user.FindFirst("email")?.Value;
        if (!string.IsNullOrWhiteSpace(email))
        {
            context.ProxyRequest.Headers.TryAddWithoutValidation(GatewayIdentityHeaders.UserEmail, email.Trim());
        }

        var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
        if (roles.Length > 0)
        {
            context.ProxyRequest.Headers.TryAddWithoutValidation(
                GatewayIdentityHeaders.UserRoles,
                string.Join(',', roles));
        }

        return ValueTask.CompletedTask;
    }
}
