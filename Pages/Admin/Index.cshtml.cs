using EventApi.Models;
using EventApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EventApi.Pages.Admin;

[Authorize(Roles = Roles.Admin)]
public class IndexModel : PageModel
{
    private readonly IAdminService _adminService;

    public IndexModel(IAdminService adminService) => _adminService = adminService;

    public AdminSummaryDto? Summary { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        try
        {
            Summary = await _adminService.GetSummaryAsync(ct);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load dashboard data: {ex.Message}";
        }
    }
}
