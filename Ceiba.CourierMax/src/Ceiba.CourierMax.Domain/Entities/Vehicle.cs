using Ceiba.CourierMax.Domain.Exceptions;

namespace Ceiba.CourierMax.Domain.Entities;

public class Vehicle
{
    public Guid Id { get; private set; }
    public string Plate { get; private set; } = string.Empty;
    public decimal MaxWeightKg { get; private set; }
    public decimal MaxVolumeM3 { get; private set; }

    public Driver? Driver { get; private set; }

    private Vehicle() { }

    public static Vehicle Create(Guid id, string plate, decimal maxWeightKg, decimal maxVolumeM3)
    {
        return new Vehicle
        {
            Id = id,
            Plate = plate,
            MaxWeightKg = maxWeightKg,
            MaxVolumeM3 = maxVolumeM3
        };
    }

    public void ValidateCapacity(decimal currentWeightKg, decimal currentVolumeM3, decimal newWeightKg, decimal newVolumeM3)
    {
        if (currentWeightKg + newWeightKg > MaxWeightKg)
            throw new DomainException(
                $"El vehículo {Plate} excede su capacidad de peso. " +
                $"Disponible: {MaxWeightKg - currentWeightKg} kg, requerido: {newWeightKg} kg.");

        if (currentVolumeM3 + newVolumeM3 > MaxVolumeM3)
            throw new DomainException(
                $"El vehículo {Plate} excede su capacidad de volumen. " +
                $"Disponible: {MaxVolumeM3 - currentVolumeM3} m³, requerido: {newVolumeM3} m³.");
    }
}
