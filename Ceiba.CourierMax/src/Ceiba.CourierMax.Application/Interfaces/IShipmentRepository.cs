using Ceiba.CourierMax.Domain.Entities;
using Ceiba.CourierMax.Domain.Enums;

namespace Ceiba.CourierMax.Application.Interfaces;

public interface IShipmentRepository
{
    Task<Shipment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Shipment?> GetByTrackingCodeAsync(string trackingCode, CancellationToken ct = default);
    Task<bool> TrackingCodeExistsAsync(string trackingCode, CancellationToken ct = default);
    Task<IReadOnlyList<Shipment>> GetByDriverIdAsync(Guid driverId, CancellationToken ct = default);
    Task<IReadOnlyList<Shipment>> GetActiveByDriverIdAsync(Guid driverId, CancellationToken ct = default);
    Task<IReadOnlyList<Shipment>> GetByStatusAsync(ShipmentStatus status, CancellationToken ct = default);
    Task<IReadOnlyList<Shipment>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Shipment shipment, CancellationToken ct = default);
    Task UpdateAsync(Shipment shipment, CancellationToken ct = default);
}
