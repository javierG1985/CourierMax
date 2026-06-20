using Ceiba.CourierMax.Domain.Entities;
using Ceiba.CourierMax.Domain.Exceptions;

namespace Ceiba.CourierMax.Tests.Unit.Domain;

public class VehicleCapacityTests
{
    private static Vehicle BuildVehicle(decimal maxWeightKg = 500, decimal maxVolumeM3 = 10) =>
        Vehicle.Create(Guid.NewGuid(), "TST-001", maxWeightKg, maxVolumeM3);

    [Fact]
    public void ValidateCapacity_WhenWithinLimits_ShouldNotThrow()
    {
        var vehicle = BuildVehicle(maxWeightKg: 500, maxVolumeM3: 10);

        var ex = Record.Exception(() =>
            vehicle.ValidateCapacity(currentWeightKg: 100, currentVolumeM3: 2, newWeightKg: 50, newVolumeM3: 1));

        Assert.Null(ex);
    }

    [Fact]
    public void ValidateCapacity_WhenExceedsWeight_ShouldThrowDomainException()
    {
        var vehicle = BuildVehicle(maxWeightKg: 500);

        Assert.Throws<DomainException>(() =>
            vehicle.ValidateCapacity(currentWeightKg: 450, currentVolumeM3: 2, newWeightKg: 60, newVolumeM3: 1));
    }

    [Fact]
    public void ValidateCapacity_WhenExceedsVolume_ShouldThrowDomainException()
    {
        var vehicle = BuildVehicle(maxVolumeM3: 10);

        Assert.Throws<DomainException>(() =>
            vehicle.ValidateCapacity(currentWeightKg: 100, currentVolumeM3: 9.5m, newWeightKg: 10, newVolumeM3: 1));
    }

    [Fact]
    public void ValidateCapacity_ExactlyAtLimit_ShouldNotThrow()
    {
        var vehicle = BuildVehicle(maxWeightKg: 500, maxVolumeM3: 10);

        var ex = Record.Exception(() =>
            vehicle.ValidateCapacity(currentWeightKg: 490, currentVolumeM3: 9, newWeightKg: 10, newVolumeM3: 1));

        Assert.Null(ex);
    }
}
