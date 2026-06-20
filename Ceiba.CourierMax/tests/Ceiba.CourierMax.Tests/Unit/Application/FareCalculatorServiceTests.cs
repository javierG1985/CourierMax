using Ceiba.CourierMax.Application.Services;
using Ceiba.CourierMax.Domain.Enums;
using Ceiba.CourierMax.Domain.Exceptions;

namespace Ceiba.CourierMax.Tests.Unit.Application;

public class FareCalculatorServiceTests
{
    // Caso del enunciado:
    // Frágil 5kg, Express, Bogotá→Medellín = $40,950
    [Fact]
    public void Calculate_ExampleFromSpec_ShouldMatch()
    {
        var fare = FareCalculatorService.Calculate(
            ServiceType.Express,
            PackageType.Fragil,
            weightKg: 5,
            City.Bogota,
            City.Medellin);

        Assert.Equal(40_950m, fare);
    }

    [Fact]
    public void Calculate_Estandar_PaqueteNormal_2kg_BogotaCali()
    {
        // Base: $8,000 + peso extra: 0 + distancia: $9,000 + recargo: 0 = $17,000
        var fare = FareCalculatorService.Calculate(
            ServiceType.Estandar,
            PackageType.Paquete,
            weightKg: 2,
            City.Bogota,
            City.Cali);

        Assert.Equal(17_000m, fare);
    }

    [Fact]
    public void Calculate_MismoDia_Perecedero_1kg_MedellinBarranquilla()
    {
        // Base: $25,000 + peso extra: 0 + distancia: $15,000 = $40,000 × 1.25 = $50,000
        var fare = FareCalculatorService.Calculate(
            ServiceType.MismoDia,
            PackageType.Perecedero,
            weightKg: 1,
            City.Medellin,
            City.Barranquilla);

        Assert.Equal(50_000m, fare);
    }

    [Fact]
    public void Calculate_WeightSurcharge_ShouldApplyAbove2kg()
    {
        // Express, Documento, 4kg, Bogotá→Cali
        // Base: $15,000 + extra: 2×$1,500=$3,000 + distancia: $9,000 = $27,000
        var fare = FareCalculatorService.Calculate(
            ServiceType.Express,
            PackageType.Documento,
            weightKg: 4,
            City.Bogota,
            City.Cali);

        Assert.Equal(27_000m, fare);
    }

    [Fact]
    public void Calculate_Bidirectional_ShouldReturnSameFare()
    {
        var fareAB = FareCalculatorService.Calculate(
            ServiceType.Express, PackageType.Paquete, 2, City.Bogota, City.Medellin);

        var fareBA = FareCalculatorService.Calculate(
            ServiceType.Express, PackageType.Paquete, 2, City.Medellin, City.Bogota);

        Assert.Equal(fareAB, fareBA);
    }

    [Fact]
    public void Calculate_WithNonExistentRoute_ShouldThrowDomainException()
    {
        // No hay ruta directa definida entre Cali y Cali
        Assert.Throws<DomainException>(() =>
            FareCalculatorService.Calculate(
                ServiceType.Express, PackageType.Paquete, 2, City.Cali, City.Cali));
    }
}
