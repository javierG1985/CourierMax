using Ceiba.CourierMax.Domain.Exceptions;
using Ceiba.CourierMax.Domain.ValueObjects;

namespace Ceiba.CourierMax.Tests.Unit.Domain;

public class PackageDimensionsTests
{
    [Fact]
    public void Create_WithValidDimensions_ShouldCalculateVolumeCorrectly()
    {
        var dims = new PackageDimensions(100, 50, 20);

        // 100 * 50 * 20 / 1_000_000 = 0.1 m³
        Assert.Equal(0.1m, dims.VolumeM3);
    }

    [Theory]
    [InlineData(0, 10, 10)]
    [InlineData(10, 0, 10)]
    [InlineData(10, 10, 0)]
    [InlineData(201, 10, 10)]
    [InlineData(10, 201, 10)]
    [InlineData(10, 10, 201)]
    public void Create_WithInvalidDimensions_ShouldThrowDomainException(decimal l, decimal w, decimal h)
    {
        Assert.Throws<DomainException>(() => new PackageDimensions(l, w, h));
    }
}
