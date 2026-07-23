using EventApi.Models;

namespace EventApi.Services;

public interface IEventService
{
    Task<IEnumerable<EventResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<EventResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<EventResponseDto> CreateAsync(
        CreateEventDto dto,
        CancellationToken cancellationToken = default);

    // Returns false when the event does not exist.
    Task<bool> UpdateAsync(
        int id,
        UpdateEventDto dto,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
