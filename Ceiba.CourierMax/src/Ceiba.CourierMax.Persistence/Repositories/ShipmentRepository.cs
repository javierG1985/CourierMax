using Ceiba.CourierMax.Domain.Entities;
using Ceiba.CourierMax.Domain.Enums;
using Ceiba.CourierMax.Domain.Repositories;
using Ceiba.CourierMax.Domain.ValueObjects;
using Ceiba.CourierMax.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Ceiba.CourierMax.Persistence.Repositories;

public sealed class ShipmentRepository(CourierMaxDbContext db) : IShipmentRepository
{
    private IQueryable<Shipment> WithIncludes =>
        db.Shipments
          .Include(s => s.StatusChanges);

    public async Task<Shipment?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await WithIncludes.FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<Shipment?> GetByTrackingCodeAsync(string trackingCode, CancellationToken ct = default)
    {
        var code = TrackingCode.Parse(trackingCode);
        return await WithIncludes.FirstOrDefaultAsync(s => s.TrackingCode == code, ct);
    }

    public async Task<bool> TrackingCodeExistsAsync(string trackingCode, CancellationToken ct = default)
    {
        var code = TrackingCode.Parse(trackingCode);
        return await db.Shipments.AnyAsync(s => s.TrackingCode == code, ct);
    }

    public async Task<IReadOnlyList<Shipment>> GetByDriverIdAsync(Guid driverId, CancellationToken ct = default) =>
        await WithIncludes.Where(s => s.AssignedDriverId == driverId).ToListAsync(ct);

    public async Task<IReadOnlyList<Shipment>> GetActiveByDriverIdAsync(Guid driverId, CancellationToken ct = default) =>
        await WithIncludes
            .Where(s => s.AssignedDriverId == driverId &&
                        (s.Status == ShipmentStatus.ASIGNADO || s.Status == ShipmentStatus.EN_TRANSITO))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Shipment>> GetByStatusAsync(ShipmentStatus status, CancellationToken ct = default) =>
        await WithIncludes.Where(s => s.Status == status).ToListAsync(ct);

    public async Task<IReadOnlyList<Shipment>> GetAllAsync(CancellationToken ct = default) =>
        await WithIncludes.ToListAsync(ct);

    public async Task AddAsync(Shipment shipment, CancellationToken ct = default)
    {
        await db.Shipments.AddAsync(shipment, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Shipment shipment, CancellationToken ct = default)
    {
        // Con UsePropertyAccessMode(Field), EF Core detecta el nuevo StatusHistory en la colección
        // pero lo trata como Modified (GUID no vacío) en lugar de Added.
        // Forzamos explícitamente el estado correcto de los nuevos registros.
        foreach (var history in shipment.StatusChanges)
        {
            var entry = db.Entry(history);
            if (entry.State is EntityState.Detached or EntityState.Modified)
            {
                var existsInDb = await db.StatusHistories.AnyAsync(h => h.Id == history.Id, ct);
                entry.State = existsInDb ? EntityState.Unchanged : EntityState.Added;
            }
        }

        await db.SaveChangesAsync(ct);
    }
}
