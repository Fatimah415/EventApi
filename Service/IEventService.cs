using EventApi.Models;

namespace EventApi.Services;

public interface IEventService
{
    Task<IEnumerable<EventResponseDto>> GetAllAsync();
    Task<EventResponseDto?> GetByIdAsync(int id);

    // Returns null when the supplied UserId does not exist.
    Task<EventResponseDto?> CreateAsync(CreateEventDto dto);

    // Returns false when the event does not exist.
    Task<bool> UpdateAsync(int id, UpdateEventDto dto);
    Task<bool> DeleteAsync(int id);
}
