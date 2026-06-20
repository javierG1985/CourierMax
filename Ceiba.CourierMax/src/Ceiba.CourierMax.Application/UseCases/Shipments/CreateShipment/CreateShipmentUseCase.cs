using AutoMapper;
using Ceiba.CourierMax.Application.DTOs.Requests;
using Ceiba.CourierMax.Application.DTOs.Responses;
using Ceiba.CourierMax.Application.Interfaces;
using Ceiba.CourierMax.Application.Services;
using Ceiba.CourierMax.Domain.Entities;
using Ceiba.CourierMax.Domain.Exceptions;
using Ceiba.CourierMax.Domain.Repositories;

namespace Ceiba.CourierMax.Application.UseCases.Shipments.CreateShipment;

public sealed class CreateShipmentUseCase(IShipmentRepository shipmentRepository, IMapper mapper) : ICreateShipmentUseCase
{
    public async Task<ShipmentResponse> ExecuteAsync(CreateShipmentRequest request, CancellationToken ct = default)
    {
        var fare = FareCalculatorService.Calculate(
            request.ServiceType,
            request.PackageType,
            request.WeightKg,
            request.OriginCity,
            request.DestinationCity);

        // Garantizar unicidad del código de rastreo (RN-05)
        Shipment shipment;
        do
        {
            shipment = Shipment.Create(
                request.SenderName, request.SenderPhone, request.SenderAddress,
                request.RecipientName, request.RecipientPhone, request.RecipientAddress,
                request.WeightKg, request.LengthCm, request.WidthCm, request.HeightCm,
                request.PackageType, request.ServiceType,
                request.OriginCity, request.DestinationCity,
                fare);
        }
        while (await shipmentRepository.TrackingCodeExistsAsync(shipment.TrackingCode.Value, ct));

        await shipmentRepository.AddAsync(shipment, ct);

        return mapper.Map<ShipmentResponse>(shipment);
    }
}
