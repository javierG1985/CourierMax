namespace Ceiba.CourierMax.Application.DTOs.Responses;

public sealed record DriverMetricsResponse(
    Guid DriverId,
    string DriverName,
    string VehiclePlate,
    int TotalAssigned,
    int TotalDelivered,
    int TotalCancelled,
    int TotalInTransit,
    double AverageDeliveryDays,
    double SlaCompliancePercentage,
    decimal TotalWeightTransportedKg
);
