using Ceiba.CourierMax.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ceiba.CourierMax.Persistence.Configurations;

public class DriverConfiguration : IEntityTypeConfiguration<Driver>
{
    public void Configure(EntityTypeBuilder<Driver> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Name).HasMaxLength(150).IsRequired();
        builder.Property(d => d.IsActive).IsRequired();

        builder.HasOne(d => d.Vehicle)
            .WithOne(v => v.Driver)
            .HasForeignKey<Driver>(d => d.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable("Drivers");
    }
}
