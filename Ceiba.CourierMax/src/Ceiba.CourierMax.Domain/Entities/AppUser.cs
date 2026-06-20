namespace Ceiba.CourierMax.Domain.Entities;

public class AppUser
{
    public Guid Id { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Role { get; private set; } = string.Empty;

    private AppUser() { }

    public static AppUser Create(Guid id, string username, string passwordHash, string role = "Admin")
        => new() { Id = id, Username = username, PasswordHash = passwordHash, Role = role };
}
