namespace EventApi.Models;

// ── User management ──────────────────────────────────────────────────────────

/// <summary>Safe user projection — no password hash exposed.</summary>
public class AdminUserDto
{
    public int    Id    { get; set; }
    public string Name  { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role  { get; set; } = string.Empty;
    public int    BookingCount { get; set; }
}

/// <summary>Request body for changing a user's role.</summary>
public class UpdateUserRoleDto
{
    public string Role { get; set; } = string.Empty;
}

// ── Booking management ───────────────────────────────────────────────────────

public class AdminBookingDto
{
    public int           Id         { get; set; }
    public int           UserId     { get; set; }
    public string        UserName   { get; set; } = string.Empty;
    public string        UserEmail  { get; set; } = string.Empty;
    public int           EventId    { get; set; }
    public string        EventTitle { get; set; } = string.Empty;
    public BookingStatus Status     { get; set; }
    public DateTime      BookedAt   { get; set; }
}

// ── Analytics ────────────────────────────────────────────────────────────────

public class BookingsPerDayDto
{
    public DateOnly Date         { get; set; }
    public int      BookingCount { get; set; }
}

public class BookingsPerMonthDto
{
    public int Year         { get; set; }
    public int Month        { get; set; }
    public int BookingCount { get; set; }
}

public class AdminSummaryDto
{
    public int TotalEvents         { get; set; }
    public int TotalUsers          { get; set; }
    public int TotalBookings       { get; set; }
    public int ConfirmedBookings   { get; set; }
    public int PendingBookings     { get; set; }
    public int CancelledBookings   { get; set; }
    public IEnumerable<BookingsPerDayDto>   BookingsPerDay   { get; set; } = [];
    public IEnumerable<BookingsPerMonthDto> BookingsPerMonth { get; set; } = [];
    public IEnumerable<BookingsPerEventDto> BookingsPerEvent { get; set; } = [];
}
