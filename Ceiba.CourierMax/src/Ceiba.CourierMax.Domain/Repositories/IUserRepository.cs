using Ceiba.CourierMax.Domain.Entities;

namespace Ceiba.CourierMax.Domain.Repositories;

public interface IUserRepository
{
    Task<AppUser?> GetByUsernameAsync(string username, CancellationToken ct = default);
}
