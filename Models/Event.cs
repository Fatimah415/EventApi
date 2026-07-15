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

    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    // Owner of the event (one User -> many Events).
    public int UserId { get; set; }

    public User? User { get; set; }

    // Server-set on creation; never supplied by the client.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
