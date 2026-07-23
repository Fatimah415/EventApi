namespace EventApi.Models;

/// <summary>
/// A grouping for events (e.g. "Conference", "Workshop"). One category has many
/// events. All constraints (required, max length, unique name, indexes) are
/// configured with the Fluent API in CategoryConfiguration — this entity stays
/// a plain POCO with no data annotations.
/// </summary>
public class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    // Navigation: Category (1) ---- (*) Events.
    public ICollection<Event> Events { get; set; } = new List<Event>();
}
