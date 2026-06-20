namespace Ceiba.CourierMax.Application.DTOs.Requests;

public sealed record AssignShipmentRequest(
    Guid ShipmentId,
    Guid DriverId,
    string AssignedBy
);
