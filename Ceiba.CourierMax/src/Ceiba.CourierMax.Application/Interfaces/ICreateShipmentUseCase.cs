using Ceiba.CourierMax.Application.DTOs.Requests;
using Ceiba.CourierMax.Application.DTOs.Responses;

namespace Ceiba.CourierMax.Application.Interfaces;

public interface ICreateShipmentUseCase
{
    Task<ShipmentResponse> ExecuteAsync(CreateShipmentRequest request, CancellationToken ct = default);
}
