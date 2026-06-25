using Ceiba.CourierMax.Application.Interfaces;
using Ceiba.CourierMax.Domain.Entities;
using Ceiba.CourierMax.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Ceiba.CourierMax.Persistence.Repositories;

public sealed class UserRepository(CourierMaxDbContext db) : IUserRepository
{
    public async Task<AppUser?> GetByUsernameAsync(string username, CancellationToken ct = default)
        => await db.Users.FirstOrDefaultAsync(u => u.Username == username, ct);
}
