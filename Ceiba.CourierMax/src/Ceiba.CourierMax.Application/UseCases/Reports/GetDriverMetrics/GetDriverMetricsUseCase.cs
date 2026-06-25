using Ceiba.CourierMax.Application.DTOs.Responses;
using Ceiba.CourierMax.Application.Interfaces;
using Ceiba.CourierMax.Domain.Enums;
using Ceiba.CourierMax.Domain.Services;

namespace Ceiba.CourierMax.Application.UseCases.Reports.GetDriverMetrics;

public sealed class GetDriverMetricsUseCase(
    IShipmentRepository shipmentRepository,
    IDriverRepository driverRepository,
    IVehicleRepository vehicleRepository) : IGetDriverMetricsUseCase
{
    private static readonly Dictionary<ServiceType, int> SlaDays = new()
    {
        { ServiceType.Estandar, 5 },
        { ServiceType.Express,  2 },
        { ServiceType.MismoDia, 0 }
    };

    public async Task<IReadOnlyList<DriverMetricsResponse>> ExecuteAsync(CancellationToken ct = default)
    {
        var drivers = await driverRepository.GetAllAsync(ct);
        var vehicles = await vehicleRepository.GetAllAsync(ct);
        var vehicleMap = vehicles.ToDictionary(v => v.Id, v => v.Plate);

        var metrics = new List<DriverMetricsResponse>();

        foreach (var driver in drivers)
        {
            var shipments = await shipmentRepository.GetByDriverIdAsync(driver.Id, ct);

            var delivered = shipments.Where(s => s.Status == ShipmentStatus.ENTREGADO).ToList();
            var cancelled = shipments.Count(s => s.Status == ShipmentStatus.CANCELADO);
            var inTransit = shipments.Count(s =>
                s.Status is ShipmentStatus.EN_TRANSITO or ShipmentStatus.ASIGNADO);

            var avgDeliveryDays = delivered.Count > 0
                ? delivered
                    .Where(s => s.AssignedAt.HasValue && s.DeliveredAt.HasValue)
                    .Select(s => (s.DeliveredAt!.Value - s.AssignedAt!.Value).TotalDays)
                    .DefaultIfEmpty(0)
                    .Average()
                : 0;

            var slaCompliant = delivered.Count > 0
                ? delivered.Count(s =>
                {
                    var sla = SlaDays[s.ServiceType];
                    var elapsed = BusinessDayService.CountBusinessDays(s.CreatedAt, s.DeliveredAt!.Value);
                    return elapsed <= sla;
                })
                : 0;

            var slaPercentage = delivered.Count > 0
                ? (double)slaCompliant / delivered.Count * 100
                : 0;

            var totalWeight = delivered.Sum(s => s.WeightKg);

            vehicleMap.TryGetValue(driver.VehicleId, out var plate);

            metrics.Add(new DriverMetricsResponse(
                driver.Id,
                driver.Name,
                plate ?? "N/A",
                shipments.Count,
                delivered.Count,
                cancelled,
                inTransit,
                Math.Round(avgDeliveryDays, 2),
                Math.Round(slaPercentage, 2),
                totalWeight
            ));
        }

        return metrics;
    }

    public async Task<DriverMetricsResponse> ExecuteForDriverAsync(Guid driverId, CancellationToken ct = default)
    {
        var all = await ExecuteAsync(ct);
        return all.FirstOrDefault(m => m.DriverId == driverId)
            ?? throw new Domain.Exceptions.DomainException($"Conductor {driverId} no encontrado.");
    }
}
