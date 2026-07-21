using EventApi.Data;
using EventApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EventApi.Repositories;

public interface IAdminRepository
{
    // Users
    Task<IEnumerable<AdminUserDto>> GetAllUsersAsync(CancellationToken ct = default);
    Task<User?> GetUserByIdAsync(int id, CancellationToken ct = default);
    Task<bool> UpdateUserRoleAsync(int id, string role, CancellationToken ct = default);

    // Bookings
    Task<IEnumerable<AdminBookingDto>> GetAllBookingsAsync(CancellationToken ct = default);

    // Analytics
    Task<AdminSummaryDto> GetSummaryAsync(CancellationToken ct = default);
}

/// <summary>
/// EF Core repository for all admin read/write operations.
/// Kept separate from EventRepository and UserRepository to maintain clear
/// separation of concerns — admin queries are aggregate/cross-table reads that
/// belong in their own slice.
/// </summary>
public class AdminRepository : IAdminRepository
{
    private readonly AppDbContext _db;

    public AdminRepository(AppDbContext db) => _db = db;

    // ── Users ─────────────────────────────────────────────────────────────────

    public async Task<IEnumerable<AdminUserDto>> GetAllUsersAsync(CancellationToken ct = default) =>
        await _db.Users
            .AsNoTracking()
            .Select(u => new AdminUserDto
            {
                Id           = u.Id,
                Name         = u.Name,
                Email        = u.Email,
                Role         = u.Role,
                BookingCount = u.Bookings.Count
            })
            .OrderBy(u => u.Id)
            .ToListAsync(ct);

    public async Task<User?> GetUserByIdAsync(int id, CancellationToken ct = default) =>
        await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<bool> UpdateUserRoleAsync(int id, string role, CancellationToken ct = default)
    {
        var user = await _db.Users.FindAsync([id], ct);
        if (user is null) return false;

        user.Role = role;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    // ── Bookings ──────────────────────────────────────────────────────────────

    public async Task<IEnumerable<AdminBookingDto>> GetAllBookingsAsync(CancellationToken ct = default) =>
        await _db.EventBookings
            .AsNoTracking()
            .Include(b => b.User)
            .Include(b => b.Event)
            .OrderByDescending(b => b.BookedAt)
            .Select(b => new AdminBookingDto
            {
                Id         = b.Id,
                UserId     = b.UserId,
                UserName   = b.User!.Name,
                UserEmail  = b.User.Email,
                EventId    = b.EventId,
                EventTitle = b.Event!.Title,
                Status     = b.Status,
                BookedAt   = b.BookedAt
            })
            .ToListAsync(ct);

    // ── Analytics ─────────────────────────────────────────────────────────────

    public async Task<AdminSummaryDto> GetSummaryAsync(CancellationToken ct = default)
    {
        var totalEvents   = await _db.Events.CountAsync(ct);
        var totalUsers    = await _db.Users.CountAsync(ct);
        var totalBookings = await _db.EventBookings.CountAsync(ct);

        var confirmed   = await _db.EventBookings.CountAsync(b => b.Status == BookingStatus.Confirmed,  ct);
        var pending     = await _db.EventBookings.CountAsync(b => b.Status == BookingStatus.Pending,    ct);
        var cancelled   = await _db.EventBookings.CountAsync(b => b.Status == BookingStatus.Cancelled,  ct);

        // Bookings per calendar day (last 30 days)
        var cutoff = DateTime.UtcNow.AddDays(-30);
        var perDay = (await _db.EventBookings
            .AsNoTracking()
            .Where(b => b.BookedAt >= cutoff)
            .GroupBy(b => new { b.BookedAt.Year, b.BookedAt.Month, b.BookedAt.Day })
            .Select(g => new { g.Key.Year, g.Key.Month, g.Key.Day, Count = g.Count() })
            .ToListAsync(ct))
            .Select(x => new BookingsPerDayDto
            {
                Date         = new DateOnly(x.Year, x.Month, x.Day),
                BookingCount = x.Count
            })
            .OrderBy(x => x.Date)
            .ToList();

        // Bookings per month (last 12 months)
        var perMonth = await _db.EventBookings
            .AsNoTracking()
            .GroupBy(b => new { b.BookedAt.Year, b.BookedAt.Month })
            .Select(g => new BookingsPerMonthDto
            {
                Year         = g.Key.Year,
                Month        = g.Key.Month,
                BookingCount = g.Count()
            })
            .OrderByDescending(x => x.Year).ThenByDescending(x => x.Month)
            .Take(12)
            .ToListAsync(ct);

        // Bookings per event (reuse existing shape)
        var perEvent = await _db.EventBookings
            .AsNoTracking()
            .Include(b => b.Event)
            .GroupBy(b => new { b.EventId, b.Event!.Title })
            .Select(g => new BookingsPerEventDto
            {
                EventId      = g.Key.EventId,
                Title        = g.Key.Title,
                BookingCount = g.Count()
            })
            .OrderByDescending(x => x.BookingCount)
            .ToListAsync(ct);

        return new AdminSummaryDto
        {
            TotalEvents       = totalEvents,
            TotalUsers        = totalUsers,
            TotalBookings     = totalBookings,
            ConfirmedBookings = confirmed,
            PendingBookings   = pending,
            CancelledBookings = cancelled,
            BookingsPerDay    = perDay,
            BookingsPerMonth  = perMonth,
            BookingsPerEvent  = perEvent
        };
    }
}
