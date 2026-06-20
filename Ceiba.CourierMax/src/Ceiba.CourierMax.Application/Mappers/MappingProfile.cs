using AutoMapper;
using Ceiba.CourierMax.Application.DTOs.Responses;
using Ceiba.CourierMax.Domain.Entities;

namespace Ceiba.CourierMax.Application.Mappers;

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        // StatusHistory → StatusHistoryResponse: todos los nombres coinciden directamente
        CreateMap<StatusHistory, StatusHistoryResponse>();

        // Shipment → ShipmentResponse: se usa ConvertUsing porque los value objects
        // (TrackingCode, PhoneNumber, Address, PackageDimensions) no se resuelven
        // automáticamente desde el constructor del record.
        CreateMap<Shipment, ShipmentResponse>().ConvertUsing(s => new ShipmentResponse(
            s.Id,
            s.TrackingCode.Value,
            s.SenderName,
            s.SenderPhone.Value,
            s.SenderAddress.Value,
            s.RecipientName,
            s.RecipientPhone.Value,
            s.RecipientAddress.Value,
            s.WeightKg,
            s.Dimensions.LengthCm,
            s.Dimensions.WidthCm,
            s.Dimensions.HeightCm,
            s.PackageType,
            s.ServiceType,
            s.OriginCity,
            s.DestinationCity,
            s.Status,
            s.Fare,
            s.CreatedAt,
            s.AssignedAt,
            s.DeliveredAt,
            s.AssignedDriverId,
            s.StatusChanges
                .Select(h => new StatusHistoryResponse(
                    h.PreviousStatus, h.NewStatus, h.ChangedAt, h.ChangedBy, h.Reason))
                .ToList()
        ));
    }
}
