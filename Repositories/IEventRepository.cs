using EventApi.Models;

namespace EventApi.Repositories;

public interface IEventRepository
{
    Task<IEnumerable<Event>> GetAllAsync();
    Task<Event?> GetByIdAsync(int id);
    Task<Event> AddAsync(Event ev);
    Task UpdateAsync(Event ev);
    Task DeleteAsync(Event ev);
    Task<bool> UserExistsAsync(int userId);
}
