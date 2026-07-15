using EventApi.Models;
using EventApi.Repositories;

namespace EventApi.Services;

public class EventService : IEventService
{
    private readonly IEventRepository _repository;

    public EventService(IEventRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<EventResponseDto>> GetAllAsync()
    {
        var events = await _repository.GetAllAsync();
        return events.Select(MapToDto);
    }

    public async Task<EventResponseDto?> GetByIdAsync(int id)
    {
        var ev = await _repository.GetByIdAsync(id);
        return ev is null ? null : MapToDto(ev);
    }

    public async Task<EventResponseDto?> CreateAsync(CreateEventDto dto)
    {
        // Owner must exist. This validation is the temporary stand-in for the
        // authenticated identity that Phase 3 (JWT) will provide instead.
        if (!await _repository.UserExistsAsync(dto.UserId))
            return null;

        var ev = new Event
        {
            Title = dto.Title,
            Description = dto.Description,
            EventDate = dto.EventDate,
            Location = dto.Location,
            Category = dto.Category,
            UserId = dto.UserId,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(ev);
        return MapToDto(ev);
    }

    public async Task<bool> UpdateAsync(int id, UpdateEventDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return false;

        existing.Title = dto.Title;
        existing.Description = dto.Description;
        existing.EventDate = dto.EventDate;
        existing.Location = dto.Location;
        existing.Category = dto.Category;
        // UserId and CreatedAt are intentionally preserved.

        await _repository.UpdateAsync(existing);
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return false;

        await _repository.DeleteAsync(existing);
        return true;
    }

    private static EventResponseDto MapToDto(Event ev) => new()
    {
        Id = ev.Id,
        Title = ev.Title,
        Description = ev.Description,
        EventDate = ev.EventDate,
        Location = ev.Location,
        Category = ev.Category,
        UserId = ev.UserId,
        CreatedAt = ev.CreatedAt
    };
}
