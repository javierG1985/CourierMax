using AutoMapper;
using Ceiba.CourierMax.Application.DTOs.Requests;
using Ceiba.CourierMax.Application.DTOs.Responses;
using Ceiba.CourierMax.Application.Interfaces;
using Ceiba.CourierMax.Domain.Exceptions;
using Ceiba.CourierMax.Domain.Repositories;

namespace Ceiba.CourierMax.Application.UseCases.Shipments.CancelShipment;

public sealed class CancelShipmentUseCase(IShipmentRepository shipmentRepository, IMapper mapper) : ICancelShipmentUseCase
{
    public async Task<ShipmentResponse> ExecuteAsync(CancelShipmentRequest request, CancellationToken ct = default)
    {
        var shipment = await shipmentRepository.GetByIdAsync(request.ShipmentId, ct)
            ?? throw new DomainException($"Envío {request.ShipmentId} no encontrado.");

        shipment.Cancel(request.Reason, request.CancelledBy);

        await shipmentRepository.UpdateAsync(shipment, ct);

        return mapper.Map<ShipmentResponse>(shipment);
    }
}
