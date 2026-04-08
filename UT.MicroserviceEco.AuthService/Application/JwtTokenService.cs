using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


using Microsoft.IdentityModel.Tokens;

using UT.MicroserviceEco.AuthService.Endpoints;
using UT.MicroserviceEco.AuthService.Infrastructure;

namespace UT.MicroserviceEco.AuthService.Application;

internal sealed class JwtTokenService(JwtOptions options)
{
    private readonly SigningCredentials _credentials =
        new(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Key)), SecurityAlgorithms.HmacSha256);

    public LoginResponse CreateToken(AppUser user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(options.ExpiryMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Name, user.UserName)
        };
        claims.AddRange(user.GetRoles().Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: _credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        return new LoginResponse(accessToken, expiresAt);
    }
}
