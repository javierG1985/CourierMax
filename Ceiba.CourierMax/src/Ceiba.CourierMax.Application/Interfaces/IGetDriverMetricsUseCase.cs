using Ceiba.CourierMax.Application.DTOs.Responses;

namespace Ceiba.CourierMax.Application.Interfaces;

public interface IGetDriverMetricsUseCase
{
    Task<IReadOnlyList<DriverMetricsResponse>> ExecuteAsync(CancellationToken ct = default);
    Task<DriverMetricsResponse> ExecuteForDriverAsync(Guid driverId, CancellationToken ct = default);
}
