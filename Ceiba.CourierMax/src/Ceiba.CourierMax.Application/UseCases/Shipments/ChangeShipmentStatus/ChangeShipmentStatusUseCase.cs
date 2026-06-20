using AutoMapper;
using Ceiba.CourierMax.Application.DTOs.Requests;
using Ceiba.CourierMax.Application.DTOs.Responses;
using Ceiba.CourierMax.Application.Interfaces;
using Ceiba.CourierMax.Domain.Exceptions;
using Ceiba.CourierMax.Domain.Repositories;

namespace Ceiba.CourierMax.Application.UseCases.Shipments.ChangeShipmentStatus;

public sealed class ChangeShipmentStatusUseCase(IShipmentRepository shipmentRepository, IMapper mapper) : IChangeShipmentStatusUseCase
{
    public async Task<ShipmentResponse> StartTransitAsync(ChangeStatusRequest request, CancellationToken ct = default)
    {
        var shipment = await shipmentRepository.GetByIdAsync(request.ShipmentId, ct)
            ?? throw new DomainException($"Envío {request.ShipmentId} no encontrado.");

        shipment.StartTransit(request.ChangedBy);

        await shipmentRepository.UpdateAsync(shipment, ct);

        return mapper.Map<ShipmentResponse>(shipment);
    }

    public async Task<ShipmentResponse> DeliverAsync(ChangeStatusRequest request, CancellationToken ct = default)
    {
        var shipment = await shipmentRepository.GetByIdAsync(request.ShipmentId, ct)
            ?? throw new DomainException($"Envío {request.ShipmentId} no encontrado.");

        shipment.Deliver(request.ChangedBy);

        await shipmentRepository.UpdateAsync(shipment, ct);

        return mapper.Map<ShipmentResponse>(shipment);
    }
}
