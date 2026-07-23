using EventApi.Models;
using EventApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var events = await _eventService.GetAllAsync(cancellationToken);
        return Ok(events);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var ev = await _eventService.GetByIdAsync(id, cancellationToken);
        if (ev is null)
            return NotFound($"Event {id} not found.");

        return Ok(ev);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Create(CreateEventDto dto, CancellationToken cancellationToken)
    {
        var created = await _eventService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Update(
        int id,
        UpdateEventDto dto,
        CancellationToken cancellationToken)
    {
        var updated = await _eventService.UpdateAsync(id, dto, cancellationToken);
        if (!updated)
            return NotFound($"Event {id} not found.");

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _eventService.DeleteAsync(id, cancellationToken);
        if (!deleted)
            return NotFound($"Event {id} not found.");

        return NoContent();
    }
}
