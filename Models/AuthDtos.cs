using System.ComponentModel.DataAnnotations;

namespace EventApi.Models;

// Request DTO for POST /api/auth/register.
public class RegisterDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
    [MaxLength(128, ErrorMessage = "Password must not exceed 128 characters.")]
    public string Password { get; set; } = string.Empty;
}

// Request DTO for POST /api/auth/login.
public class LoginDto
{
    [Required]
    [MaxLength(150)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string Password { get; set; } = string.Empty;
}

// Response DTO returned by a successful login.
public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
