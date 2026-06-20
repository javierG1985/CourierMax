using Ceiba.CourierMax.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace Ceiba.CourierMax.Domain.ValueObjects;

public sealed record PhoneNumber
{
    public string Value { get; }

    public PhoneNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !Regex.IsMatch(value, @"^[36]\d{9}$"))
            throw new DomainException("El teléfono debe tener 10 dígitos y comenzar con 3 o 6.");

        Value = value;
    }

    public override string ToString() => Value;
}
