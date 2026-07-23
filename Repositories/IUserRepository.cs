using EventApi.Models;

namespace EventApi.Repositories;

public interface IUserRepository
{
    Task AddUserAsync(User user);
    Task<User?> GetByEmailAsync(string email);
    Task<bool> UserExistsAsync(string email);
}
