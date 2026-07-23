using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EventApi.Models;
using Microsoft.IdentityModel.Tokens;

namespace EventApi.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _config;

    public JwtTokenService(IConfiguration config)
    {
        _config = config;
    }

    public (string Token, DateTime ExpiresAt) GenerateToken(User user)
    {
        var keyValue = _config["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key is not configured.");
        var issuer = _config["Jwt:Issuer"]
            ?? throw new InvalidOperationException("Jwt:Issuer is not configured.");
        var audience = _config["Jwt:Audience"]
            ?? throw new InvalidOperationException("Jwt:Audience is not configured.");

        if (Encoding.UTF8.GetByteCount(keyValue) < 32)
            throw new InvalidOperationException("Jwt:Key must be at least 32 bytes for HS256.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyValue));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiryMinutes = int.TryParse(_config["Jwt:ExpiryMinutes"], out var configuredMinutes)
            ? configuredMinutes
            : 60;
        if (expiryMinutes is < 1 or > 1440)
            throw new InvalidOperationException("Jwt:ExpiryMinutes must be between 1 and 1440.");

        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        // ClaimTypes.Role maps to ASP.NET Core's default role claim type, so
        // [Authorize(Roles = "...")] works without extra configuration.
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
