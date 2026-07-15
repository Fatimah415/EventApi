using EventApi.Data;
using EventApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EventApi.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> RegisterUserAsync(UserDto userDto)
    {
        // Reject duplicate emails (also enforced by a unique index in the DB).
        var emailExists = await _context.Users
            .AnyAsync(u => u.Email == userDto.Email);

        if (emailExists)
            return false;

        var user = new User
        {
            Name = userDto.Name,
            Email = userDto.Email,
            // Never store the plaintext password — hash it with BCrypt.
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return true;
    }
}
