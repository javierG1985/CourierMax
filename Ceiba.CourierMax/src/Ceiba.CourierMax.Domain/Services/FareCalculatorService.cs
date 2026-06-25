using Ceiba.CourierMax.Domain.Enums;

namespace Ceiba.CourierMax.Domain.Services;

public static class FareCalculatorService
{
    private const decimal WeightSurchargePerKg = 1_500m;
    private const decimal FreeWeightKg = 2m;

    private static readonly Dictionary<ServiceType, decimal> BaseFares = new()
    {
        { ServiceType.Estandar, 8_000m },
        { ServiceType.Express,  15_000m },
        { ServiceType.MismoDia, 25_000m }
    };

    private static readonly Dictionary<PackageType, decimal> PackageSurchargeRate = new()
    {
        { PackageType.Fragil,     0.30m },
        { PackageType.Perecedero, 0.25m },
        { PackageType.Documento,  0m },
        { PackageType.Paquete,    0m }
    };

    public static decimal Calculate(
        ServiceType serviceType,
        PackageType packageType,
        decimal weightKg,
        City originCity,
        City destinationCity)
    {
        var baseFare = BaseFares[serviceType];

        var extraWeight = Math.Max(0, weightKg - FreeWeightKg);
        var weightSurcharge = extraWeight * WeightSurchargePerKg;

        var distanceFare = CityRouteService.GetDistanceFare(originCity, destinationCity);

        var subtotal = baseFare + weightSurcharge + distanceFare;

        var packageRate = PackageSurchargeRate[packageType];
        var packageSurcharge = subtotal * packageRate;

        return subtotal + packageSurcharge;
    }
}
