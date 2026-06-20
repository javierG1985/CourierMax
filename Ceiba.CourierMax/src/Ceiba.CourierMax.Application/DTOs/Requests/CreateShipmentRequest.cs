using Ceiba.CourierMax.Domain.Enums;

namespace Ceiba.CourierMax.Application.DTOs.Requests;

public sealed record CreateShipmentRequest(
    string SenderName,
    string SenderPhone,
    string SenderAddress,
    string RecipientName,
    string RecipientPhone,
    string RecipientAddress,
    decimal WeightKg,
    decimal LengthCm,
    decimal WidthCm,
    decimal HeightCm,
    PackageType PackageType,
    ServiceType ServiceType,
    City OriginCity,
    City DestinationCity
);
