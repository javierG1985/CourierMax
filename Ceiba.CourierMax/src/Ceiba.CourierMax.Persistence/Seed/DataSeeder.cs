using Ceiba.CourierMax.Domain.Entities;
using Ceiba.CourierMax.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Ceiba.CourierMax.Persistence.Seed;

public static class DataSeeder
{
    public static readonly Guid VehicleAbc123Id = new("11111111-0000-0000-0000-000000000001");
    public static readonly Guid VehicleDef456Id = new("11111111-0000-0000-0000-000000000002");
    public static readonly Guid VehicleGhi789Id = new("11111111-0000-0000-0000-000000000003");

    public static readonly Guid DriverJuanId   = new("22222222-0000-0000-0000-000000000001");
    public static readonly Guid DriverMariaId  = new("22222222-0000-0000-0000-000000000002");
    public static readonly Guid DriverCarlosId = new("22222222-0000-0000-0000-000000000003");

    public static readonly Guid AdminUserId = new("33333333-0000-0000-0000-000000000001");

    public static async Task SeedAsync(CourierMaxDbContext db)
    {
        if (!await db.Vehicles.AnyAsync())
        {
            var vehicles = new[]
            {
                Vehicle.Create(VehicleAbc123Id, "ABC-123", maxWeightKg: 500, maxVolumeM3: 10),
                Vehicle.Create(VehicleDef456Id, "DEF-456", maxWeightKg: 300, maxVolumeM3: 6),
                Vehicle.Create(VehicleGhi789Id, "GHI-789", maxWeightKg: 800, maxVolumeM3: 15)
            };
            await db.Vehicles.AddRangeAsync(vehicles);
            await db.SaveChangesAsync();

            var drivers = new[]
            {
                Driver.Create(DriverJuanId,   "Juan Pérez",  VehicleAbc123Id),
                Driver.Create(DriverMariaId,  "María López", VehicleDef456Id),
                Driver.Create(DriverCarlosId, "Carlos Ruiz", VehicleGhi789Id)
            };
            await db.Drivers.AddRangeAsync(drivers);
            await db.SaveChangesAsync();
        }

        if (!await db.Users.AnyAsync())
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!");
            var admin = AppUser.Create(AdminUserId, "admin", passwordHash, role: "Admin");
            await db.Users.AddAsync(admin);
            await db.SaveChangesAsync();
        }
    }
}
