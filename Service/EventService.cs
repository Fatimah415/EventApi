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

    public async Task<IEnumerable<EventResponseDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var events = await _repository.GetAllAsync(cancellationToken);
        return events.Select(MapToDto);
    }

    public async Task<EventResponseDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        ValidateId(id, nameof(id));

        var ev = await _repository.GetByIdAsync(id, cancellationToken);
        return ev is null ? null : MapToDto(ev);
    }

    public async Task<EventResponseDto> CreateAsync(
        CreateEventDto dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ValidateId(dto.UserId, nameof(dto.UserId));
        ValidateId(dto.CategoryId, nameof(dto.CategoryId));

        if (!await _repository.UserExistsAsync(dto.UserId, cancellationToken))
            throw new KeyNotFoundException($"User {dto.UserId} does not exist.");

        if (!await _repository.CategoryExistsAsync(dto.CategoryId, cancellationToken))
            throw new KeyNotFoundException($"Category {dto.CategoryId} does not exist.");

        var ev = new Event
        {
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            EventDate = dto.EventDate,
            Location = dto.Location.Trim(),
            CategoryId = dto.CategoryId,
            UserId = dto.UserId,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(ev, cancellationToken);
        return MapToDto(ev);
    }

    public async Task<bool> UpdateAsync(
        int id,
        UpdateEventDto dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ValidateId(id, nameof(id));
        ValidateId(dto.CategoryId, nameof(dto.CategoryId));

        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
            return false;

        if (!await _repository.CategoryExistsAsync(dto.CategoryId, cancellationToken))
            throw new KeyNotFoundException($"Category {dto.CategoryId} does not exist.");

        existing.Title = dto.Title.Trim();
        existing.Description = dto.Description?.Trim();
        existing.EventDate = dto.EventDate;
        existing.Location = dto.Location.Trim();
        existing.CategoryId = dto.CategoryId;
        // UserId and CreatedAt are intentionally preserved.

        await _repository.UpdateAsync(existing, cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        ValidateId(id, nameof(id));

        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
            return false;

        await _repository.DeleteAsync(existing, cancellationToken);
        return true;
    }

    private static void ValidateId(int id, string parameterName)
    {
        if (id <= 0)
            throw new ArgumentException("The identifier must be greater than zero.", parameterName);
    }

    private static EventResponseDto MapToDto(Event ev) => new()
    {
        Id = ev.Id,
        Title = ev.Title,
        Description = ev.Description,
        EventDate = ev.EventDate,
        Location = ev.Location,
        CategoryId = ev.CategoryId,
        CategoryName = ev.Category?.Name,
        UserId = ev.UserId,
        CreatedAt = ev.CreatedAt
    };
}
