using EventApi.Models;

namespace EventApi.Services;

public class UserService : IUserService
{
    private static readonly List<User> Users = new();

    public Task<bool> RegisterUserAsync(UserDto userDto)
    {
        if (Users.Any(u => u.Email == userDto.Email))
            return Task.FromResult(false);

        Users.Add(new User
        {
            Id = Users.Count + 1,
            Name = userDto.Name,
            Email = userDto.Email,
            Password = userDto.Password
        });

        return Task.FromResult(true);
    }
}