using Ceiba.CourierMax.Application.DTOs.Requests;
using Ceiba.CourierMax.Application.DTOs.Responses;

namespace Ceiba.CourierMax.Application.Interfaces;

public interface IAssignShipmentUseCase
{
    Task<ShipmentResponse> ExecuteAsync(AssignShipmentRequest request, CancellationToken ct = default);
}
