namespace Ceiba.CourierMax.Application.DTOs.Requests;

public sealed record CancelShipmentRequest(
    Guid ShipmentId,
    string Reason,
    string CancelledBy
);
