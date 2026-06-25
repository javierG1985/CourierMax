using AutoMapper;
using Ceiba.CourierMax.Application.DTOs.Requests;
using Ceiba.CourierMax.Application.DTOs.Responses;
using Ceiba.CourierMax.Application.Interfaces;
using Ceiba.CourierMax.Domain.Exceptions;

namespace Ceiba.CourierMax.Application.UseCases.Shipments.AssignShipment;

public sealed class AssignShipmentUseCase(
    IShipmentRepository shipmentRepository,
    IDriverRepository driverRepository,
    IMapper mapper) : IAssignShipmentUseCase
{
    public async Task<ShipmentResponse> ExecuteAsync(AssignShipmentRequest request, CancellationToken ct = default)
    {
        var shipment = await shipmentRepository.GetByIdAsync(request.ShipmentId, ct)
            ?? throw new DomainException($"Envío {request.ShipmentId} no encontrado.");

        var driver = await driverRepository.GetByIdAsync(request.DriverId, ct)
            ?? throw new DomainException($"Conductor {request.DriverId} no encontrado.");

        if (!driver.IsActive)
            throw new DomainException($"El conductor {driver.Name} no está activo.");

        // El vehículo viene incluido desde el driver (DriverRepository hace Include(d => d.Vehicle))
        var vehicle = driver.Vehicle
            ?? throw new DomainException($"Vehículo del conductor no encontrado.");

        var activeShipments = await shipmentRepository.GetActiveByDriverIdAsync(driver.Id, ct);

        var currentWeightKg = activeShipments.Sum(s => s.WeightKg);
        var currentVolumeM3 = activeShipments.Sum(s => s.Dimensions.VolumeM3);

        vehicle.ValidateCapacity(
            currentWeightKg, currentVolumeM3,
            shipment.WeightKg, shipment.Dimensions.VolumeM3);

        shipment.AssignToDriver(driver.Id, request.AssignedBy);

        await shipmentRepository.UpdateAsync(shipment, ct);

        return mapper.Map<ShipmentResponse>(shipment);
    }
}
