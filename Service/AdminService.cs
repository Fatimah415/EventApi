using EventApi.Models;
using EventApi.Repositories;

namespace EventApi.Services;

public interface IAdminService
{
    // Users
    Task<IEnumerable<AdminUserDto>> GetAllUsersAsync(CancellationToken ct = default);
    Task<AdminUserDto?> GetUserByIdAsync(int id, CancellationToken ct = default);
    Task<bool> UpdateUserRoleAsync(int id, string role, CancellationToken ct = default);

    // Bookings
    Task<IEnumerable<AdminBookingDto>> GetAllBookingsAsync(CancellationToken ct = default);

    // Analytics
    Task<AdminSummaryDto> GetSummaryAsync(CancellationToken ct = default);
}

/// <summary>
/// Thin service layer that sits between AdminController and AdminRepository.
/// Validates business rules (e.g. only known roles are accepted) so the
/// controller stays free of domain logic.
/// </summary>
public class AdminService : IAdminService
{
    private readonly IAdminRepository _repo;
    private readonly ILogger<AdminService> _logger;

    private static readonly HashSet<string> ValidRoles =
        [Roles.Admin, Roles.User];

    public AdminService(IAdminRepository repo, ILogger<AdminService> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    // ── Users ─────────────────────────────────────────────────────────────────

    public Task<IEnumerable<AdminUserDto>> GetAllUsersAsync(CancellationToken ct = default) =>
        _repo.GetAllUsersAsync(ct);

    public async Task<AdminUserDto?> GetUserByIdAsync(int id, CancellationToken ct = default)
    {
        var user = await _repo.GetUserByIdAsync(id, ct);
        if (user is null) return null;

        return new AdminUserDto
        {
            Id    = user.Id,
            Name  = user.Name,
            Email = user.Email,
            Role  = user.Role
        };
    }

    /// <summary>
    /// Role update. Returns null if the role name is invalid to produce a 400,
    /// false if the user is not found (404), true on success.
    /// </summary>
    public async Task<bool> UpdateUserRoleAsync(int id, string role, CancellationToken ct = default)
    {
        if (!ValidRoles.Contains(role))
            throw new ArgumentException($"Invalid role '{role}'. Valid roles: {string.Join(", ", ValidRoles)}.");

        var updated = await _repo.UpdateUserRoleAsync(id, role, ct);

        if (updated)
            _logger.LogInformation("Admin updated User {UserId} role to {Role}.", id, role);

        return updated;
    }

    // ── Bookings ──────────────────────────────────────────────────────────────

    public Task<IEnumerable<AdminBookingDto>> GetAllBookingsAsync(CancellationToken ct = default) =>
        _repo.GetAllBookingsAsync(ct);

    // ── Analytics ─────────────────────────────────────────────────────────────

    public Task<AdminSummaryDto> GetSummaryAsync(CancellationToken ct = default) =>
        _repo.GetSummaryAsync(ct);
}
