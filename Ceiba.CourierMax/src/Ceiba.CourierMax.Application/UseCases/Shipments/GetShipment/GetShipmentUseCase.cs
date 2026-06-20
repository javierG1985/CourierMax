using AutoMapper;
using Ceiba.CourierMax.Application.DTOs.Responses;
using Ceiba.CourierMax.Application.Interfaces;
using Ceiba.CourierMax.Domain.Exceptions;
using Ceiba.CourierMax.Domain.Repositories;

namespace Ceiba.CourierMax.Application.UseCases.Shipments.GetShipment;

public sealed class GetShipmentUseCase(IShipmentRepository shipmentRepository, IMapper mapper) : IGetShipmentUseCase
{
    public async Task<ShipmentResponse> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var shipment = await shipmentRepository.GetByIdAsync(id, ct)
            ?? throw new DomainException($"Envío {id} no encontrado.");

        return mapper.Map<ShipmentResponse>(shipment);
    }

    public async Task<ShipmentResponse> GetByTrackingCodeAsync(string trackingCode, CancellationToken ct = default)
    {
        var shipment = await shipmentRepository.GetByTrackingCodeAsync(trackingCode, ct)
            ?? throw new DomainException($"Envío con código {trackingCode} no encontrado.");

        return mapper.Map<ShipmentResponse>(shipment);
    }

    public async Task<IReadOnlyList<ShipmentResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var shipments = await shipmentRepository.GetAllAsync(ct);
        return mapper.Map<IReadOnlyList<ShipmentResponse>>(shipments);
    }
}
