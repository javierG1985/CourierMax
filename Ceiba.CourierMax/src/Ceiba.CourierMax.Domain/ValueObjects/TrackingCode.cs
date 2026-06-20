using Ceiba.CourierMax.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace Ceiba.CourierMax.Domain.ValueObjects;

public sealed record TrackingCode
{
    public string Value { get; }

    private TrackingCode(string value) => Value = value;

    public static TrackingCode Generate()
    {
        var digits = Random.Shared.Next(0, 99_999_999).ToString("D8");
        return new TrackingCode($"CM{digits}");
    }

    public static TrackingCode Parse(string value)
    {
        if (!Regex.IsMatch(value, @"^CM\d{8}$"))
            throw new DomainException($"Código de rastreo inválido: {value}");

        return new TrackingCode(value);
    }

    public override string ToString() => Value;
}
