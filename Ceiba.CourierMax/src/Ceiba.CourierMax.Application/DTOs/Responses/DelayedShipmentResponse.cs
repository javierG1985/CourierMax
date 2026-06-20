using Ceiba.CourierMax.Domain.Enums;

namespace Ceiba.CourierMax.Application.DTOs.Responses;

public sealed record DelayedShipmentResponse(
    Guid Id,
    string TrackingCode,
    ServiceType ServiceType,
    ShipmentStatus Status,
    DateTime CreatedAt,
    int SlaBusinessDays,
    int ElapsedBusinessDays,
    int DaysOverdue,
    Guid? AssignedDriverId,
    string? AssignedDriverName
);
