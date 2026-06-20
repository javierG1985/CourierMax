using Ceiba.CourierMax.Domain.Exceptions;
using Ceiba.CourierMax.Domain.ValueObjects;

namespace Ceiba.CourierMax.Tests.Unit.Domain;

public class TrackingCodeTests
{
    [Fact]
    public void Generate_ShouldProduceValidFormat()
    {
        var code = TrackingCode.Generate();
        Assert.Matches(@"^CM\d{8}$", code.Value);
    }

    [Fact]
    public void Parse_WithValidCode_ShouldSucceed()
    {
        var code = TrackingCode.Parse("CM12345678");
        Assert.Equal("CM12345678", code.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("CM1234")]
    [InlineData("XX12345678")]
    [InlineData("CM1234567A")]
    public void Parse_WithInvalidCode_ShouldThrowDomainException(string invalid)
    {
        Assert.Throws<DomainException>(() => TrackingCode.Parse(invalid));
    }

    [Fact]
    public void TwoCodes_WithSameValue_ShouldBeEqual()
    {
        var a = TrackingCode.Parse("CM00000001");
        var b = TrackingCode.Parse("CM00000001");
        Assert.Equal(a, b);
    }
}
