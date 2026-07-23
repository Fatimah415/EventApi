using EventApi.Models;

namespace EventApi.Repositories;

public interface IUserRepository
{
    Task AddUserAsync(User user, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> UserExistsAsync(string email, CancellationToken cancellationToken = default);
}
