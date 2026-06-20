using Ceiba.CourierMax.Domain.Enums;

namespace Ceiba.CourierMax.Application.DTOs.Responses;

public sealed record ShipmentResponse(
    Guid Id,
    string TrackingCode,
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
    City DestinationCity,
    ShipmentStatus Status,
    decimal Fare,
    DateTime CreatedAt,
    DateTime? AssignedAt,
    DateTime? DeliveredAt,
    Guid? AssignedDriverId,
    IReadOnlyList<StatusHistoryResponse> StatusChanges
);
