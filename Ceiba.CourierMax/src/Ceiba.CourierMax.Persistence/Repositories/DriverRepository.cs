using Ceiba.CourierMax.Domain.Entities;
using Ceiba.CourierMax.Domain.Repositories;
using Ceiba.CourierMax.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Ceiba.CourierMax.Persistence.Repositories;

public sealed class DriverRepository(CourierMaxDbContext db) : IDriverRepository
{
    public async Task<Driver?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await db.Drivers.Include(d => d.Vehicle).FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task<IReadOnlyList<Driver>> GetAllActiveAsync(CancellationToken ct = default) =>
        await db.Drivers.Include(d => d.Vehicle).Where(d => d.IsActive).ToListAsync(ct);

    public async Task<IReadOnlyList<Driver>> GetAllAsync(CancellationToken ct = default) =>
        await db.Drivers.Include(d => d.Vehicle).ToListAsync(ct);
}
