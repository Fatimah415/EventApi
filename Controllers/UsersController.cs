using Microsoft.AspNetCore.Mvc;
using EventApi.Models;
using EventApi.Services;

namespace EventApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserDto userDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _userService.RegisterUserAsync(userDto);

        if (!result)
            return Conflict("Email already exists.");

        return Ok("User registered successfully.");
    }
}