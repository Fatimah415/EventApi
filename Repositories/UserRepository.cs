using EventApi.Data;
using EventApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EventApi.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddUserAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<bool> UserExistsAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        return await _context.Users.AnyAsync(u => u.Email == email, cancellationToken);
    }
}
