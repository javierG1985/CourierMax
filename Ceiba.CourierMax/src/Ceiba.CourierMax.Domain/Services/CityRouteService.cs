using Ceiba.CourierMax.Domain.Enums;
using Ceiba.CourierMax.Domain.Exceptions;

namespace Ceiba.CourierMax.Domain.Services;

public static class CityRouteService
{
    // Tarifa de distancia entre pares de ciudades (bidireccional)
    private static readonly Dictionary<(City, City), decimal> DistanceFares = new()
    {
        { (City.Bogota, City.Medellin), 12_000m },
        { (City.Bogota, City.Cali), 9_000m },
        { (City.Bogota, City.Barranquilla), 20_000m },
        { (City.Medellin, City.Cali), 8_000m },
        { (City.Medellin, City.Barranquilla), 15_000m },
        { (City.Cali, City.Barranquilla), 18_000m }
    };

    public static decimal GetDistanceFare(City origin, City destination)
    {
        if (DistanceFares.TryGetValue((origin, destination), out var fare))
            return fare;

        if (DistanceFares.TryGetValue((destination, origin), out fare))
            return fare;

        throw new DomainException($"No existe ruta definida entre {origin} y {destination}.");
    }
}
