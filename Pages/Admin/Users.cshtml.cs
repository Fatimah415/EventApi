using EventApi.Models;
using EventApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EventApi.Pages.Admin;

[Authorize(Roles = Roles.Admin)]
public class UsersModel : PageModel
{
    private readonly IAdminService _adminService;

    public UsersModel(IAdminService adminService) => _adminService = adminService;

    public IEnumerable<AdminUserDto> Users { get; private set; } = [];
    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        try
        {
            Users = await _adminService.GetAllUsersAsync(ct);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load users: {ex.Message}";
        }
    }

    public async Task<IActionResult> OnPostUpdateRoleAsync(int id, string role, CancellationToken ct)
    {
        try
        {
            await _adminService.UpdateUserRoleAsync(id, role, ct);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Role update failed: {ex.Message}";
        }
        return RedirectToPage();
    }
}
