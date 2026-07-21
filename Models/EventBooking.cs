namespace EventApi.Models;

/// <summary>
/// A reservation made by a user for an event.
/// User (1) ---- (*) EventBooking and Event (1) ---- (*) EventBooking.
/// Constraints, foreign keys, delete behavior, the BookingStatus string
/// conversion and indexes are all configured in EventBookingConfiguration
/// (Fluent API) — this entity stays a plain POCO.
/// </summary>
public class EventBooking
{
    public int Id { get; set; }

    // FK -> User (the person who booked).
    public int UserId { get; set; }
    public User? User { get; set; }

    // FK -> Event (the event being booked).
    public int EventId { get; set; }
    public Event? Event { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    // Server-set when the booking is created.
    public DateTime BookedAt { get; set; } = DateTime.UtcNow;
}
