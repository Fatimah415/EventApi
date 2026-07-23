using System.Security.Claims;
using EventApi.Data;
using EventApi.Models;
using EventApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EventApi.Pages.Admin.Events;

[Authorize(Roles = Roles.Admin)]
public class CreateModel : PageModel
{
    private readonly IEventService _eventService;
    private readonly AppDbContext _db;

    public CreateModel(IEventService eventService, AppDbContext db)
    {
        _eventService = eventService;
        _db = db;
    }

    [BindProperty]
    public CreateEventDto Input { get; set; } = new();

    public List<Category> Categories { get; set; } = new();

    public async Task OnGetAsync()
    {
        Input.EventDate = DateTime.Now.AddDays(1);
        Categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
            return Page();
        }

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdStr, out var userId))
        {
            ModelState.AddModelError(string.Empty, "Could not determine your User ID. Please log in again.");
            Categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
            return Page();
        }

        Input.UserId = userId;

        try
        {
            var result = await _eventService.CreateAsync(Input);
            if (result == null)
            {
                ModelState.AddModelError(string.Empty, "Failed to create event. User ID invalid?");
                Categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
                return Page();
            }

            return RedirectToPage("/Admin/Events");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error creating event: {ex.Message}");
            Categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
            return Page();
        }
    }
}
