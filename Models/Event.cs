using System.ComponentModel.DataAnnotations;

namespace EventApi.Models;

public class Event
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public DateTime EventDate { get; set; }

    [Required]
    [MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    // FK -> Category. Replaces the old free-text Category string so events are
    // grouped through a real relationship (Category 1 ---- * Events).
    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    // Owner of the event (one User -> many Events).
    public int UserId { get; set; }

    public User? User { get; set; }

    // Inverse navigations for the new relationships.
    public ICollection<EventBooking> Bookings { get; set; } = new List<EventBooking>();
    public ICollection<EventFavorite> Favorites { get; set; } = new List<EventFavorite>();

    // Server-set on creation; never supplied by the client.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
