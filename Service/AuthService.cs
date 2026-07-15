using EventApi.Models;
using EventApi.Repositories;

namespace EventApi.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(IUserRepository userRepository, IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<int?> RegisterAsync(RegisterDto dto)
    {
        if (await _userRepository.UserExistsAsync(dto.Email))
            return null;

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            // Never store the plaintext password — hash it with BCrypt.
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = "User"
        };

        await _userRepository.AddUserAsync(user);
        return user.Id;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email);

        // Same null result for unknown email and wrong password so the
        // response never reveals which one was incorrect.
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return null;

        var (token, expiresAt) = _jwtTokenService.GenerateToken(user);
        return new LoginResponseDto { Token = token, ExpiresAt = expiresAt };
    }
}
