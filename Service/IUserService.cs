using EventApi.Models;

namespace EventApi.Services;

public interface IUserService
{
    Task<bool> RegisterUserAsync(UserDto userDto);
}