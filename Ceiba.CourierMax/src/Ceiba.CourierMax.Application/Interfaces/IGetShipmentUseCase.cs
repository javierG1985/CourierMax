using Ceiba.CourierMax.Application.DTOs.Responses;

namespace Ceiba.CourierMax.Application.Interfaces;

public interface IGetShipmentUseCase
{
    Task<ShipmentResponse> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ShipmentResponse> GetByTrackingCodeAsync(string trackingCode, CancellationToken ct = default);
    Task<IReadOnlyList<ShipmentResponse>> GetAllAsync(CancellationToken ct = default);
}
