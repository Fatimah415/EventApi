using EventApi.Models;
using EventApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto, CancellationToken cancellationToken)
    {
        var userId = await _authService.RegisterAsync(dto, cancellationToken);
        if (userId is null)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Email already in use."
            });
        }

        return StatusCode(
            StatusCodes.Status201Created,
            new { UserId = userId, Message = "User registered successfully." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto, CancellationToken cancellationToken)
    {
        var response = await _authService.LoginAsync(dto, cancellationToken);
        if (response is null)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Invalid credentials."
            });
        }

        return Ok(response);
    }
}
