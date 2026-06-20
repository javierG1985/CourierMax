using Ceiba.CourierMax.Domain.Entities;
using Ceiba.CourierMax.Domain.Enums;
using Ceiba.CourierMax.Domain.Exceptions;

namespace Ceiba.CourierMax.Tests.Unit.Domain;

public class ShipmentTests
{
    private static Shipment BuildShipment(
        ServiceType service = ServiceType.Express,
        PackageType package = PackageType.Paquete,
        decimal weight = 5m) =>
        Shipment.Create(
            "Remitente Test", "3001234567", "Calle 1 #10-20",
            "Destinatario Test", "3109876543", "Carrera 5 #20-30",
            weight, 30, 20, 15,
            package, service,
            City.Bogota, City.Medellin,
            fare: 40_950m);

    [Fact]
    public void Create_ShouldHaveStatusCreado()
    {
        var shipment = BuildShipment();
        Assert.Equal(ShipmentStatus.CREADO, shipment.Status);
    }

    [Fact]
    public void Create_ShouldGenerateTrackingCode()
    {
        var shipment = BuildShipment();
        Assert.Matches(@"^CM\d{8}$", shipment.TrackingCode.Value);
    }

    [Fact]
    public void Create_WithSameOriginAndDestination_ShouldThrow()
    {
        Assert.Throws<DomainException>(() =>
            Shipment.Create(
                "A", "3001234567", "Dir A",
                "B", "3109876543", "Dir B",
                5, 30, 20, 15,
                PackageType.Paquete, ServiceType.Express,
                City.Bogota, City.Bogota, 0));
    }

    [Theory]
    [InlineData(0.09)]
    [InlineData(100.01)]
    public void Create_WithInvalidWeight_ShouldThrow(double weight)
    {
        Assert.Throws<DomainException>(() =>
            Shipment.Create(
                "A", "3001234567", "Dir A",
                "B", "3109876543", "Dir B",
                (decimal)weight, 30, 20, 15,
                PackageType.Paquete, ServiceType.Express,
                City.Bogota, City.Medellin, 0));
    }

    [Fact]
    public void AssignToDriver_FromCreado_ShouldTransitionToAsignado()
    {
        var shipment = BuildShipment();
        shipment.AssignToDriver(Guid.NewGuid(), "operador1");

        Assert.Equal(ShipmentStatus.ASIGNADO, shipment.Status);
        Assert.NotNull(shipment.AssignedAt);
        Assert.Single(shipment.StatusChanges.Where(h => h.NewStatus == ShipmentStatus.ASIGNADO));
    }

    [Fact]
    public void StartTransit_FromAsignado_ShouldTransitionToEnTransito()
    {
        var shipment = BuildShipment();
        shipment.AssignToDriver(Guid.NewGuid(), "op1");
        shipment.StartTransit("op1");

        Assert.Equal(ShipmentStatus.EN_TRANSITO, shipment.Status);
    }

    [Fact]
    public void StartTransit_FromCreado_ShouldThrow()
    {
        var shipment = BuildShipment();

        Assert.Throws<DomainException>(() => shipment.StartTransit("op1"));
    }

    [Fact]
    public void Deliver_FromEnTransito_ShouldTransitionToEntregado()
    {
        var shipment = BuildShipment();
        shipment.AssignToDriver(Guid.NewGuid(), "op1");
        shipment.StartTransit("op1");
        shipment.Deliver("op1");

        Assert.Equal(ShipmentStatus.ENTREGADO, shipment.Status);
        Assert.NotNull(shipment.DeliveredAt);
    }

    [Fact]
    public void Deliver_FromAsignado_ShouldThrow()
    {
        var shipment = BuildShipment();
        shipment.AssignToDriver(Guid.NewGuid(), "op1");

        Assert.Throws<DomainException>(() => shipment.Deliver("op1"));
    }

    [Fact]
    public void Cancel_FromCualquierEstado_ExceptoEntregado_ShouldSucceed()
    {
        var shipment = BuildShipment();
        shipment.AssignToDriver(Guid.NewGuid(), "op1");
        shipment.StartTransit("op1");
        shipment.Cancel("Motivo de cancelación válido", "op1");

        Assert.Equal(ShipmentStatus.CANCELADO, shipment.Status);
    }

    [Fact]
    public void Cancel_WhenEntregado_ShouldThrow()
    {
        var shipment = BuildShipment();
        shipment.AssignToDriver(Guid.NewGuid(), "op1");
        shipment.StartTransit("op1");
        shipment.Deliver("op1");

        Assert.Throws<DomainException>(() => shipment.Cancel("Motivo válido", "op1"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    [InlineData("   ")]
    public void Cancel_WithShortReason_ShouldThrow(string reason)
    {
        var shipment = BuildShipment();
        Assert.Throws<DomainException>(() => shipment.Cancel(reason, "op1"));
    }

    [Fact]
    public void StatusChanges_ShouldRecordFullHistory()
    {
        var shipment = BuildShipment();
        var driverId = Guid.NewGuid();
        shipment.AssignToDriver(driverId, "op1");
        shipment.StartTransit("op1");
        shipment.Deliver("op1");

        // CREADO + ASIGNADO + EN_TRANSITO + ENTREGADO
        Assert.Equal(4, shipment.StatusChanges.Count);
    }
}
