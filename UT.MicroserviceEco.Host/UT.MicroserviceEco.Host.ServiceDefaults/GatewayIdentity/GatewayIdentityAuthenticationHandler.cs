using System.Security.Claims;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace UT.MicroserviceEco.Host.ServiceDefaults.GatewayIdentity;

internal sealed class GatewayIdentityAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var userName = Request.Headers[GatewayIdentityHeaders.UserName].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(userName))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        userName = userName.Trim();
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, userName),
            new("unique_name", userName)
        };

        var email = Request.Headers[GatewayIdentityHeaders.UserEmail].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(email))
        {
            claims.Add(new Claim("email", email.Trim()));
        }

        var rolesHeader = Request.Headers[GatewayIdentityHeaders.UserRoles].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(rolesHeader))
        {
            foreach (var role in rolesHeader.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        var identity = new ClaimsIdentity(claims, Scheme.Name, ClaimTypes.Name, ClaimTypes.Role);
        var principal = new ClaimsPrincipal(identity);
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name)));
    }
}
