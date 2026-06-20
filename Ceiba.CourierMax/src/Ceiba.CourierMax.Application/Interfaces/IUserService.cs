using Ceiba.CourierMax.Domain.Entities;

namespace Ceiba.CourierMax.Application.Interfaces;

public interface IUserService
{
    Task<AppUser?> FindByUsernameAsync(string username, CancellationToken ct = default);
    bool VerifyPassword(string plainPassword, string passwordHash);
}
