using Ceiba.CourierMax.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ceiba.CourierMax.Persistence.Context;

public class CourierMaxDbContext(DbContextOptions<CourierMaxDbContext> options) : DbContext(options)
{
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<StatusHistory> StatusHistories => Set<StatusHistory>();
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<AppUser> Users => Set<AppUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CourierMaxDbContext).Assembly);
    }
}
