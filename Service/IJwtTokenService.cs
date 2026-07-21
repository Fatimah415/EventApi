using EventApi.Models;

namespace EventApi.Services;

public interface IJwtTokenService
{
    // Returns the signed JWT and its UTC expiry time.
    (string Token, DateTime ExpiresAt) GenerateToken(User user);
}
