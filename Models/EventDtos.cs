using System.ComponentModel.DataAnnotations;

namespace EventApi.Models;

// Request DTO for creating an event.
public class CreateEventDto
{
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

    // Which category this event belongs to (FK -> Category).
    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    // The admin explicitly chooses the event organizer.
    [Range(1, int.MaxValue)]
    public int UserId { get; set; }
}

// Request DTO for updating an event (owner and CreatedAt are not editable).
public class UpdateEventDto
{
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

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }
}

// Response DTO returned to clients (avoids exposing the entity / navigation).
public class EventResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    // Convenience: the category's name, when the navigation is loaded.
    public string? CategoryName { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; }
}
