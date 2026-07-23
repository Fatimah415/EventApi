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
    private readonly IWeatherService _weatherService;

    public EventsController(IEventService eventService, IWeatherService weatherService)
    {
        _eventService   = eventService;
        _weatherService = weatherService;
    }

    // GET api/events — unchanged
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var events = await _eventService.GetAllAsync();
        return Ok(events);
    }

    // GET api/events/{id} — unchanged
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var ev = await _eventService.GetByIdAsync(id);
        if (ev is null)
            return NotFound($"Event {id} not found.");

        return Ok(ev);
    }

    // GET api/events/{id}/weather
    // Fetches current weather for the event's location from OpenWeatherMap.
    // This is a read-only cross-service query — the controller coordinates
    // EventService (load event) and WeatherService (fetch weather) without
    // adding business logic to either service.
    [HttpGet("{id}/weather")]
    [AllowAnonymous]
    public async Task<IActionResult> GetWeather(int id)
    {
        // 1. Load the event to get its location.
        var ev = await _eventService.GetByIdAsync(id);
        if (ev is null)
            return NotFound($"Event {id} not found.");

        // 2. Validate that the event has a usable location.
        if (string.IsNullOrWhiteSpace(ev.Location))
            return BadRequest($"Event {id} does not have a location.");

        // 3. Fetch weather — all errors (invalid city, API failures, timeouts)
        //    are thrown as exceptions and handled by GlobalExceptionHandler.
        var weather = await _weatherService.GetWeatherAsync(ev.Location, HttpContext.RequestAborted);
        return Ok(weather);
    }

    // POST api/events
    // [FromForm] replaces the implicit [FromBody] so that ASP.NET Core reads the
    // request as multipart/form-data, which is the only content type that can
    // carry both text fields and an IFormFile in the same request.
    // [FromBody] only reads application/json and cannot bind file uploads.
    [HttpPost]
    [Authorize(Roles = Roles.Admin, AuthenticationSchemes = "Bearer")]
    public async Task<IActionResult> Create([FromForm] CreateEventDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _eventService.CreateAsync(dto);
        if (created is null)
            return BadRequest($"User {dto.UserId} does not exist.");

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT api/events/{id}
    // Same reason as Create: [FromForm] is required to bind IFormFile alongside
    // the other text properties. Validation, business logic, and file handling
    // are all delegated to EventService — the controller stays thin.
    [HttpPut("{id}")]
    [Authorize(Roles = Roles.Admin, AuthenticationSchemes = "Bearer")]
    public async Task<IActionResult> Update(int id, [FromForm] UpdateEventDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = await _eventService.UpdateAsync(id, dto);
        if (!updated)
            return NotFound($"Event {id} not found.");

        return NoContent();
    }

    // DELETE api/events/{id} — unchanged
    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.Admin, AuthenticationSchemes = "Bearer")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _eventService.DeleteAsync(id);
        if (!deleted)
            return NotFound($"Event {id} not found.");

        return NoContent();
    }
}
