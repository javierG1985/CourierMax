namespace Ceiba.CourierMax.Application.DTOs.Requests;

public sealed record ChangeStatusRequest(
    Guid ShipmentId,
    string ChangedBy
);
