using Ceiba.CourierMax.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ceiba.CourierMax.Persistence.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Plate).HasMaxLength(20).IsRequired();
        builder.HasIndex(v => v.Plate).IsUnique();
        builder.Property(v => v.MaxWeightKg).HasColumnType("TEXT").IsRequired();
        builder.Property(v => v.MaxVolumeM3).HasColumnType("TEXT").IsRequired();

        builder.ToTable("Vehicles");
    }
}
