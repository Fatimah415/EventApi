namespace EventApi.Models;

/// <summary>
/// Lifecycle state of an <see cref="EventBooking"/>. Stored in the database as a
/// string (configured via HasConversion in EventBookingConfiguration) so the
/// column is human-readable and safe against future reordering of these members.
/// </summary>
public enum BookingStatus
{
    Pending,
    Confirmed,
    Cancelled
}
