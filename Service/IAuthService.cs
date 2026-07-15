using EventApi.Models;

namespace EventApi.Services;

public interface IAuthService
{
    // Returns the new user's Id, or null when the email is already registered.
    Task<int?> RegisterAsync(RegisterDto dto);

    // Returns a token response, or null when the credentials are invalid.
    Task<LoginResponseDto?> LoginAsync(LoginDto dto);
}
