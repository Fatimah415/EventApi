namespace EventApi.Models;

/// <summary>
/// Explicit join table for the many-to-many "favorites" relationship:
/// User (*) ---- (*) Event. The primary key is the composite (UserId, EventId),
/// which also prevents a user from favoriting the same event twice. The
/// composite key, foreign keys, delete behavior and indexes are configured in
/// EventFavoriteConfiguration (Fluent API).
/// </summary>
public class EventFavorite
{
    // FK -> User. Part of the composite primary key.
    public int UserId { get; set; }
    public User? User { get; set; }

    // FK -> Event. Part of the composite primary key.
    public int EventId { get; set; }
    public Event? Event { get; set; }

    // Server-set when the favorite is created.
    public DateTime FavoritedAt { get; set; } = DateTime.UtcNow;
}
