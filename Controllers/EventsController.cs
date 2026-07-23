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
    public async Task<IActionResult> GetAll()
    {
        var events = await _eventService.GetAllAsync();
        return Ok(events);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var ev = await _eventService.GetByIdAsync(id);
        if (ev is null)
            return NotFound($"Event {id} not found.");

        return Ok(ev);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Create(CreateEventDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _eventService.CreateAsync(dto);
        if (created is null)
            return BadRequest($"User {dto.UserId} does not exist.");

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Update(int id, UpdateEventDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = await _eventService.UpdateAsync(id, dto);
        if (!updated)
            return NotFound($"Event {id} not found.");

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _eventService.DeleteAsync(id);
        if (!deleted)
            return NotFound($"Event {id} not found.");

        return NoContent();
    }
}
