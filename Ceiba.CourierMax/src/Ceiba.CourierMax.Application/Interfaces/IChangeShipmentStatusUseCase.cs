using Ceiba.CourierMax.Application.DTOs.Requests;
using Ceiba.CourierMax.Application.DTOs.Responses;

namespace Ceiba.CourierMax.Application.Interfaces;

public interface IChangeShipmentStatusUseCase
{
    Task<ShipmentResponse> StartTransitAsync(ChangeStatusRequest request, CancellationToken ct = default);
    Task<ShipmentResponse> DeliverAsync(ChangeStatusRequest request, CancellationToken ct = default);
}
