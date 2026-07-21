namespace EventApi.Models;

/// <summary>
/// One row of the "Bookings per Event" report: an event and how many bookings
/// it has (0 for events with none, thanks to the LEFT JOIN).
/// </summary>
public class BookingsPerEventDto
{
    public int EventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int BookingCount { get; set; }
}
