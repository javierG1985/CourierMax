using Ceiba.CourierMax.Application.Interfaces;
using Ceiba.CourierMax.Domain.Entities;
using Ceiba.CourierMax.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Ceiba.CourierMax.Persistence.Repositories;

public sealed class VehicleRepository(CourierMaxDbContext db) : IVehicleRepository
{
    public async Task<Vehicle?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await db.Vehicles.FindAsync([id], ct);

    public async Task<IReadOnlyList<Vehicle>> GetAllAsync(CancellationToken ct = default) =>
        await db.Vehicles.ToListAsync(ct);
}
