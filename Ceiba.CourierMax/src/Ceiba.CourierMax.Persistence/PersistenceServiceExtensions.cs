using Ceiba.CourierMax.Domain.Repositories;
using Ceiba.CourierMax.Persistence.Context;
using Ceiba.CourierMax.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Ceiba.CourierMax.Persistence;

public static class PersistenceServiceExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<CourierMaxDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IShipmentRepository, ShipmentRepository>();
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<IDriverRepository, DriverRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
