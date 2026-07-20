using System.Security.Claims;
using EventApi.Models;
using EventApi.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EventApi.Pages.Admin;

/// <summary>
/// Cookie-based login for the Razor Pages admin panel.
/// The existing JWT auth works for the API layer; Razor Pages needs
/// cookie auth because browsers cannot set Authorization headers.
/// Both schemes coexist: JWT handles /api/*, cookies handle /Admin/*.
/// </summary>
public class LoginModel : PageModel
{
    private readonly IAuthService _authService;

    public LoginModel(IAuthService authService) => _authService = authService;

    [BindProperty] public string Email    { get; set; } = string.Empty;
    [BindProperty] public string Password { get; set; } = string.Empty;
    public string? ErrorMessage { get; private set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        // Reuse the existing AuthService — same BCrypt verification, same DB.
        var result = await _authService.LoginAsync(new LoginDto
        {
            Email    = Email,
            Password = Password
        });

        if (result is null)
        {
            ErrorMessage = "Invalid email or password.";
            return Page();
        }

        // We need the user's role to check Admin access.
        // Parse it from the JWT that AuthService already produced.
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwt     = handler.ReadJwtToken(result.Token);
        var role    = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value
                   ?? jwt.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

        var userId  = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
                   ?? jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

        if (role != Roles.Admin)
        {
            ErrorMessage = "Access denied. Admin role required.";
            return Page();
        }

        // Issue a cookie so subsequent Razor Page requests are authenticated.
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email,   Email),
            new(ClaimTypes.Role,    Roles.Admin),
            new(ClaimTypes.Name,    Email),
            new(ClaimTypes.NameIdentifier, userId ?? "")
        };

        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = false });

        return RedirectToPage("/Admin/Index");
    }
}
