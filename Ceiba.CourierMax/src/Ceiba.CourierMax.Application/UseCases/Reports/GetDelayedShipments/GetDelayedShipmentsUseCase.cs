using Ceiba.CourierMax.Application.DTOs.Responses;
using Ceiba.CourierMax.Application.Interfaces;
using Ceiba.CourierMax.Domain.Enums;
using Ceiba.CourierMax.Domain.Services;

namespace Ceiba.CourierMax.Application.UseCases.Reports.GetDelayedShipments;

public sealed class GetDelayedShipmentsUseCase(
    IShipmentRepository shipmentRepository,
    IDriverRepository driverRepository) : IGetDelayedShipmentsUseCase
{
    private static readonly Dictionary<ServiceType, int> SlaDays = new()
    {
        { ServiceType.Estandar, 5 },
        { ServiceType.Express,  2 },
        { ServiceType.MismoDia, 0 }
    };

    public async Task<IReadOnlyList<DelayedShipmentResponse>> ExecuteAsync(
        DateTime from, DateTime to, CancellationToken ct = default)
    {
        var allShipments = await shipmentRepository.GetAllAsync(ct);
        var drivers = await driverRepository.GetAllAsync(ct);
        var driverMap = drivers.ToDictionary(d => d.Id, d => d.Name);

        var now = DateTime.UtcNow;
        var delayed = new List<DelayedShipmentResponse>();

        foreach (var shipment in allShipments)
        {
            // Solo envíos no entregados creados dentro del rango
            if (shipment.Status == ShipmentStatus.ENTREGADO) continue;
            if (shipment.CreatedAt < from || shipment.CreatedAt > to) continue;

            var sla = SlaDays[shipment.ServiceType];
            var elapsed = BusinessDayService.CountBusinessDays(shipment.CreatedAt, now);

            if (elapsed <= sla) continue;

            driverMap.TryGetValue(shipment.AssignedDriverId ?? Guid.Empty, out var driverName);

            delayed.Add(new DelayedShipmentResponse(
                shipment.Id,
                shipment.TrackingCode.Value,
                shipment.ServiceType,
                shipment.Status,
                shipment.CreatedAt,
                sla,
                elapsed,
                elapsed - sla,
                shipment.AssignedDriverId,
                driverName
            ));
        }

        return delayed.OrderByDescending(d => d.DaysOverdue).ToList();
    }
}
