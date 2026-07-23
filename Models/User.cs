using System.ComponentModel.DataAnnotations;

namespace EventApi.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    // Stores a BCrypt HASH of the password, never the plaintext value.
    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    // Authorization role: defaults to the standard user role. See Roles.
    [Required]
    [MaxLength(50)]
    public string Role { get; set; } = Roles.User;

    // Inverse navigations. A user has many bookings (User 1 ---- * EventBooking)
    // and many favorited events (User * ---- * Event via EventFavorite).
    public ICollection<EventBooking> Bookings { get; set; } = new List<EventBooking>();
    public ICollection<EventFavorite> Favorites { get; set; } = new List<EventFavorite>();
}
