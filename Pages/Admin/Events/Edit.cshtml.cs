using EventApi.Data;
using EventApi.Models;
using EventApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EventApi.Pages.Admin.Events;

[Authorize(Roles = Roles.Admin)]
public class EditModel : PageModel
{
    private readonly IEventService _eventService;
    private readonly AppDbContext _db;

    public EditModel(IEventService eventService, AppDbContext db)
    {
        _eventService = eventService;
        _db = db;
    }

    [BindProperty]
    public UpdateEventDto Input { get; set; } = new();

    [BindProperty]
    public int EventId { get; set; }

    public List<Category> Categories { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        EventId = id;
        Categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
        
        var ev = await _eventService.GetByIdAsync(id);
        if (ev == null)
        {
            return RedirectToPage("/Admin/Events");
        }

        Input = new UpdateEventDto
        {
            Title = ev.Title,
            Description = ev.Description,
            EventDate = ev.EventDate,
            Location = ev.Location,
            CategoryId = ev.CategoryId
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
            return Page();
        }

        try
        {
            var success = await _eventService.UpdateAsync(EventId, Input);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Failed to update event (not found?).");
                Categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
                return Page();
            }

            return RedirectToPage("/Admin/Events");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error updating event: {ex.Message}");
            Categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
            return Page();
        }
    }
}
