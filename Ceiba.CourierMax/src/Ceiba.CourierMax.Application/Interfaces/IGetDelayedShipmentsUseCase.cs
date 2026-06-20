using Ceiba.CourierMax.Application.DTOs.Responses;

namespace Ceiba.CourierMax.Application.Interfaces;

public interface IGetDelayedShipmentsUseCase
{
    Task<IReadOnlyList<DelayedShipmentResponse>> ExecuteAsync(DateTime from, DateTime to, CancellationToken ct = default);
}
