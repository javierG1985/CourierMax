using Ceiba.CourierMax.Domain.Exceptions;
using Ceiba.CourierMax.Domain.ValueObjects;

namespace Ceiba.CourierMax.Tests.Unit.Domain;

public class PhoneNumberTests
{
    [Theory]
    [InlineData("3001234567")]
    [InlineData("3219876543")]
    [InlineData("6011234567")]
    public void Create_WithValidPhone_ShouldSucceed(string phone)
    {
        var result = new PhoneNumber(phone);
        Assert.Equal(phone, result.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("123456789")]
    [InlineData("4001234567")]
    [InlineData("30012345678")]
    [InlineData("300123456")]
    public void Create_WithInvalidPhone_ShouldThrowDomainException(string phone)
    {
        Assert.Throws<DomainException>(() => new PhoneNumber(phone));
    }
}
