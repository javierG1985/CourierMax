using Ceiba.CourierMax.Domain.Enums;

namespace Ceiba.CourierMax.Domain.Entities;

public class StatusHistory
{
    public Guid Id { get; private set; }
    public Guid ShipmentId { get; private set; }
    public ShipmentStatus PreviousStatus { get; private set; }
    public ShipmentStatus NewStatus { get; private set; }
    public DateTime ChangedAt { get; private set; }
    public string? Reason { get; private set; }
    public string ChangedBy { get; private set; } = string.Empty;

    private StatusHistory() { }

    public static StatusHistory Create(
        Guid shipmentId,
        ShipmentStatus previousStatus,
        ShipmentStatus newStatus,
        string changedBy,
        string? reason = null)
    {
        return new StatusHistory
        {
            Id = Guid.NewGuid(),
            ShipmentId = shipmentId,
            PreviousStatus = previousStatus,
            NewStatus = newStatus,
            ChangedAt = DateTime.UtcNow,
            ChangedBy = changedBy,
            Reason = reason
        };
    }
}
