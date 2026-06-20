using Ceiba.CourierMax.API.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Ceiba.CourierMax.API.Services;

public sealed class JwtTokenService(IConfiguration config, IHttpContextAccessor httpContextAccessor) : IJwtTokenService
{
    private const string CookieName = "access_token";

    public void IssueToken(string username, string role)
    {
        var token = GenerateJwt(username, role);

        // HttpOnly, Secure y SameSite son aplicados por la CookiePolicy global (DependencyInjectionService)
        httpContextAccessor.HttpContext!.Response.Cookies.Append(CookieName, token, new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddHours(config.GetValue<int>("Jwt:ExpirationHours", 8))
        });
    }

    public void RevokeToken()
        => httpContextAccessor.HttpContext!.Response.Cookies.Delete(CookieName);

    private string GenerateJwt(string username, string role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:SecretKey"]!));

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(config.GetValue<int>("Jwt:ExpirationHours", 8)),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
