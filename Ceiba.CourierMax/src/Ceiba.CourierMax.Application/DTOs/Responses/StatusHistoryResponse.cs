using Ceiba.CourierMax.Domain.Enums;

namespace Ceiba.CourierMax.Application.DTOs.Responses;

public sealed record StatusHistoryResponse(
    ShipmentStatus PreviousStatus,
    ShipmentStatus NewStatus,
    DateTime ChangedAt,
    string ChangedBy,
    string? Reason
);
