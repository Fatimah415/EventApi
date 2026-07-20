using EventApi.Models;
using EventApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EventApi.Pages.Admin;

[Authorize(Roles = Roles.Admin)]
public class BookingsModel : PageModel
{
    private readonly IAdminService _adminService;

    public BookingsModel(IAdminService adminService) => _adminService = adminService;

    public IEnumerable<AdminBookingDto> Bookings { get; private set; } = [];
    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        try
        {
            Bookings = await _adminService.GetAllBookingsAsync(ct);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load bookings: {ex.Message}";
        }
    }
}
