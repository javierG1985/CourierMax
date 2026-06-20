using Ceiba.CourierMax.Domain.Enums;
using Ceiba.CourierMax.Domain.Exceptions;
using Ceiba.CourierMax.Domain.ValueObjects;

namespace Ceiba.CourierMax.Domain.Entities;

public class Shipment
{
    public Guid Id { get; private set; }
    public TrackingCode TrackingCode { get; private set; } = null!;

    // Remitente
    public string SenderName { get; private set; } = string.Empty;
    public PhoneNumber SenderPhone { get; private set; } = null!;
    public Address SenderAddress { get; private set; } = null!;

    // Destinatario
    public string RecipientName { get; private set; } = string.Empty;
    public PhoneNumber RecipientPhone { get; private set; } = null!;
    public Address RecipientAddress { get; private set; } = null!;

    // Paquete
    public decimal WeightKg { get; private set; }
    public PackageDimensions Dimensions { get; private set; } = null!;
    public PackageType PackageType { get; private set; }

    // Servicio y ruta
    public ServiceType ServiceType { get; private set; }
    public City OriginCity { get; private set; }
    public City DestinationCity { get; private set; }

    // Estado
    public ShipmentStatus Status { get; private set; }
    public decimal Fare { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? AssignedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }

    // Asignación
    public Guid? AssignedDriverId { get; private set; }

    private readonly List<StatusHistory> _statusHistory = [];
    public IReadOnlyCollection<StatusHistory> StatusChanges => _statusHistory.AsReadOnly();

    private Shipment() { }

    public static Shipment Create(
        string senderName, string senderPhone, string senderAddress,
        string recipientName, string recipientPhone, string recipientAddress,
        decimal weightKg, decimal lengthCm, decimal widthCm, decimal heightCm,
        PackageType packageType, ServiceType serviceType,
        City originCity, City destinationCity,
        decimal fare)
    {
        if (originCity == destinationCity)
            throw new DomainException("La ciudad de origen y destino no pueden ser la misma.");

        if (weightKg < 0.1m || weightKg > 100m)
            throw new DomainException("El peso debe estar entre 0.1 kg y 100 kg.");

        var shipment = new Shipment
        {
            Id = Guid.NewGuid(),
            TrackingCode = TrackingCode.Generate(),
            SenderName = senderName,
            SenderPhone = new PhoneNumber(senderPhone),
            SenderAddress = new Address(senderAddress),
            RecipientName = recipientName,
            RecipientPhone = new PhoneNumber(recipientPhone),
            RecipientAddress = new Address(recipientAddress),
            WeightKg = weightKg,
            Dimensions = new PackageDimensions(lengthCm, widthCm, heightCm),
            PackageType = packageType,
            ServiceType = serviceType,
            OriginCity = originCity,
            DestinationCity = destinationCity,
            Fare = fare,
            Status = ShipmentStatus.CREADO,
            CreatedAt = DateTime.UtcNow
        };

        shipment._statusHistory.Add(StatusHistory.Create(
            shipment.Id, ShipmentStatus.CREADO, ShipmentStatus.CREADO, "system"));

        return shipment;
    }

    public void AssignToDriver(Guid driverId, string changedBy)
    {
        EnsureNotDelivered();
        EnsureNotCancelled();

        var previous = Status;
        Status = ShipmentStatus.ASIGNADO;
        AssignedDriverId = driverId;
        AssignedAt = DateTime.UtcNow;

        _statusHistory.Add(StatusHistory.Create(Id, previous, Status, changedBy));
    }

    public void StartTransit(string changedBy)
    {
        if (Status != ShipmentStatus.ASIGNADO)
            throw new DomainException($"Solo se puede iniciar tránsito desde estado ASIGNADO. Estado actual: {Status}.");

        var previous = Status;
        Status = ShipmentStatus.EN_TRANSITO;
        _statusHistory.Add(StatusHistory.Create(Id, previous, Status, changedBy));
    }

    public void Deliver(string changedBy)
    {
        if (Status != ShipmentStatus.EN_TRANSITO)
            throw new DomainException($"Solo se puede entregar desde estado EN_TRANSITO. Estado actual: {Status}.");

        var previous = Status;
        Status = ShipmentStatus.ENTREGADO;
        DeliveredAt = DateTime.UtcNow;
        _statusHistory.Add(StatusHistory.Create(Id, previous, Status, changedBy));
    }

    public void Cancel(string reason, string changedBy)
    {
        EnsureNotDelivered();

        if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length < 5)
            throw new DomainException("El motivo de cancelación debe tener al menos 5 caracteres.");

        var previous = Status;
        Status = ShipmentStatus.CANCELADO;
        _statusHistory.Add(StatusHistory.Create(Id, previous, Status, changedBy, reason));
    }

    public void ReleaseFromDriver()
    {
        AssignedDriverId = null;
    }

    private void EnsureNotDelivered()
    {
        if (Status == ShipmentStatus.ENTREGADO)
            throw new DomainException("No se puede modificar un envío ya entregado.");
    }

    private void EnsureNotCancelled()
    {
        if (Status == ShipmentStatus.CANCELADO)
            throw new DomainException("No se puede modificar un envío cancelado.");
    }
}
