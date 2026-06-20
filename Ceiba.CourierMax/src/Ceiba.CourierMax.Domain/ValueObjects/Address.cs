using Ceiba.CourierMax.Domain.Exceptions;

namespace Ceiba.CourierMax.Domain.ValueObjects;

public sealed record Address
{
    public string Value { get; }

    public Address(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("La dirección no puede estar vacía.");

        Value = value.Trim();
    }

    public override string ToString() => Value;
}
