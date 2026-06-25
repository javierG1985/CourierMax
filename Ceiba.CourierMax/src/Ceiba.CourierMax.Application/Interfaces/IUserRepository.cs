using Ceiba.CourierMax.Domain.Entities;

namespace Ceiba.CourierMax.Application.Interfaces;

public interface IUserRepository
{
    Task<AppUser?> GetByUsernameAsync(string username, CancellationToken ct = default);
}
