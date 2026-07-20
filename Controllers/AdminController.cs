using EventApi.Models;
using EventApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventApi.Controllers;

/// <summary>
/// Admin-only controller. Every endpoint requires a valid JWT with Role = "Admin".
/// Provides: event management (delegated to EventsController services), user
/// management, booking listing, and analytics.
/// </summary>
[ApiController]
[Route("api/admin")]
[Authorize(Roles = Roles.Admin)]
public class AdminController : ControllerBase
{
    private readonly IAdminService  _adminService;
    private readonly IEventService  _eventService;

    public AdminController(IAdminService adminService, IEventService eventService)
    {
        _adminService = adminService;
        _eventService = eventService;
    }

    // ── Event management ─────────────────────────────────────────────────────

    /// <summary>GET api/admin/events — list all events.</summary>
    [HttpGet("events")]
    public async Task<IActionResult> GetEvents(CancellationToken ct)
    {
        var events = await _eventService.GetAllAsync();
        return Ok(events);
    }

    /// <summary>GET api/admin/events/{id} — single event detail.</summary>
    [HttpGet("events/{id:int}")]
    public async Task<IActionResult> GetEvent(int id)
    {
        var ev = await _eventService.GetByIdAsync(id);
        return ev is null ? NotFound($"Event {id} not found.") : Ok(ev);
    }

    /// <summary>POST api/admin/events — create an event.</summary>
    [HttpPost("events")]
    public async Task<IActionResult> CreateEvent([FromForm] CreateEventDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var created = await _eventService.CreateAsync(dto);
        if (created is null)
            return BadRequest($"User {dto.UserId} does not exist.");

        return CreatedAtAction(nameof(GetEvent), new { id = created.Id }, created);
    }

    /// <summary>PUT api/admin/events/{id} — update an event.</summary>
    [HttpPut("events/{id:int}")]
    public async Task<IActionResult> UpdateEvent(int id, [FromForm] UpdateEventDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var updated = await _eventService.UpdateAsync(id, dto);
        return updated ? NoContent() : NotFound($"Event {id} not found.");
    }

    /// <summary>DELETE api/admin/events/{id} — delete an event.</summary>
    [HttpDelete("events/{id:int}")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var deleted = await _eventService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound($"Event {id} not found.");
    }

    // ── User management ──────────────────────────────────────────────────────

    /// <summary>GET api/admin/users — list all users (no password data).</summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(CancellationToken ct)
    {
        var users = await _adminService.GetAllUsersAsync(ct);
        return Ok(users);
    }

    /// <summary>GET api/admin/users/{id} — single user detail.</summary>
    [HttpGet("users/{id:int}")]
    public async Task<IActionResult> GetUser(int id, CancellationToken ct)
    {
        var user = await _adminService.GetUserByIdAsync(id, ct);
        return user is null ? NotFound($"User {id} not found.") : Ok(user);
    }

    /// <summary>
    /// PATCH api/admin/users/{id}/role — change a user's role.
    /// Accepted values: "Admin", "User".
    /// ArgumentException (invalid role) is caught by GlobalExceptionHandler → 400.
    /// </summary>
    [HttpPatch("users/{id:int}/role")]
    public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateUserRoleDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Role))
            return BadRequest("Role must not be empty.");

        var updated = await _adminService.UpdateUserRoleAsync(id, dto.Role, ct);
        return updated ? NoContent() : NotFound($"User {id} not found.");
    }

    // ── Booking management ───────────────────────────────────────────────────

    /// <summary>GET api/admin/bookings — list all bookings with user and event info.</summary>
    [HttpGet("bookings")]
    public async Task<IActionResult> GetBookings(CancellationToken ct)
    {
        var bookings = await _adminService.GetAllBookingsAsync(ct);
        return Ok(bookings);
    }

    // ── Analytics ────────────────────────────────────────────────────────────

    /// <summary>
    /// GET api/admin/analytics — dashboard summary.
    /// Returns: totals, bookings-per-day (last 30 days), bookings-per-month
    /// (last 12 months), and bookings-per-event (all time).
    /// </summary>
    [HttpGet("analytics")]
    public async Task<IActionResult> GetAnalytics(CancellationToken ct)
    {
        var summary = await _adminService.GetSummaryAsync(ct);
        return Ok(summary);
    }
}
