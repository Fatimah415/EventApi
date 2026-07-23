using EventApi.Models;

namespace EventApi.Repositories;

public interface IEventRepository
{
    Task<IEnumerable<Event>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Event?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Event> AddAsync(Event ev, CancellationToken cancellationToken = default);
    Task UpdateAsync(Event ev, CancellationToken cancellationToken = default);
    Task DeleteAsync(Event ev, CancellationToken cancellationToken = default);
    Task<bool> UserExistsAsync(int userId, CancellationToken cancellationToken = default);
    Task<bool> CategoryExistsAsync(int categoryId, CancellationToken cancellationToken = default);
}
