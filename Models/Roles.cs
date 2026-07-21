namespace EventApi.Models;

/// <summary>
/// Central definitions for authorization role names. Using constants keeps the
/// role strings in one place so <c>[Authorize(Roles = ...)]</c> attributes, the
/// <see cref="User"/> default, and registration all stay in sync.
/// </summary>
public static class Roles
{
    public const string User = "User";
    public const string Admin = "Admin";
}
