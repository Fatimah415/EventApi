using EventApi.Models;
using EventApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EventApi.Pages.Admin;

[Authorize(Roles = Roles.Admin)]
public class EventsModel : PageModel
{
    private readonly IEventService _eventService;

    public EventsModel(IEventService eventService) => _eventService = eventService;

    public IEnumerable<EventResponseDto> Events { get; private set; } = [];
    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync()
    {
        try
        {
            Events = await _eventService.GetAllAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load events: {ex.Message}";
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        try
        {
            await _eventService.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Delete failed: {ex.Message}";
        }
        return RedirectToPage();
    }
}
