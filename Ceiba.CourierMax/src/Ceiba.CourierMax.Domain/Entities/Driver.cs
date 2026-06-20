namespace Ceiba.CourierMax.Domain.Entities;

public class Driver
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public Guid VehicleId { get; private set; }

    public Vehicle Vehicle { get; private set; } = null!;

    private Driver() { }

    public static Driver Create(Guid id, string name, Guid vehicleId)
    {
        return new Driver
        {
            Id = id,
            Name = name,
            VehicleId = vehicleId,
            IsActive = true
        };
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
