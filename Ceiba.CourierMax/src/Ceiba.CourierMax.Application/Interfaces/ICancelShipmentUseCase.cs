using Ceiba.CourierMax.Application.DTOs.Requests;
using Ceiba.CourierMax.Application.DTOs.Responses;

namespace Ceiba.CourierMax.Application.Interfaces;

public interface ICancelShipmentUseCase
{
    Task<ShipmentResponse> ExecuteAsync(CancelShipmentRequest request, CancellationToken ct = default);
}
