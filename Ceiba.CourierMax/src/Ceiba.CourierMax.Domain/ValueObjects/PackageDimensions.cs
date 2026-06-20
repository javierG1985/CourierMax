using Ceiba.CourierMax.Domain.Exceptions;

namespace Ceiba.CourierMax.Domain.ValueObjects;

public sealed record PackageDimensions
{
    public decimal LengthCm { get; }
    public decimal WidthCm { get; }
    public decimal HeightCm { get; }

    public PackageDimensions(decimal lengthCm, decimal widthCm, decimal heightCm)
    {
        Validate(lengthCm, "Largo");
        Validate(widthCm, "Ancho");
        Validate(heightCm, "Alto");

        LengthCm = lengthCm;
        WidthCm = widthCm;
        HeightCm = heightCm;
    }

    // Volume in m³
    public decimal VolumeM3 => LengthCm * WidthCm * HeightCm / 1_000_000m;

    private static void Validate(decimal value, string field)
    {
        if (value < 1 || value > 200)
            throw new DomainException($"{field}: debe estar entre 1 cm y 200 cm.");
    }
}
