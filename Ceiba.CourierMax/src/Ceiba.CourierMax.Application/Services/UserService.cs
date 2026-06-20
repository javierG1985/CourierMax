using Ceiba.CourierMax.Application.Interfaces;
using Ceiba.CourierMax.Domain.Entities;
using Ceiba.CourierMax.Domain.Repositories;

namespace Ceiba.CourierMax.Application.Services;

public sealed class UserService(IUserRepository userRepository) : IUserService
{
    public Task<AppUser?> FindByUsernameAsync(string username, CancellationToken ct = default)
        => userRepository.GetByUsernameAsync(username, ct);

    public bool VerifyPassword(string plainPassword, string passwordHash)
        => BCrypt.Net.BCrypt.Verify(plainPassword, passwordHash);
}
