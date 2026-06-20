using Ceiba.CourierMax.Domain.Entities;
using Ceiba.CourierMax.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ceiba.CourierMax.Persistence.Configurations;

public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.HasKey(s => s.Id);

        // TrackingCode → string column con índice único
        builder.Property(s => s.TrackingCode)
            .HasConversion(tc => tc.Value, v => TrackingCode.Parse(v))
            .HasMaxLength(10)
            .IsRequired();

        builder.HasIndex(s => s.TrackingCode).IsUnique();

        // PhoneNumber → string
        builder.Property(s => s.SenderPhone)
            .HasConversion(p => p.Value, v => new PhoneNumber(v))
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(s => s.RecipientPhone)
            .HasConversion(p => p.Value, v => new PhoneNumber(v))
            .HasMaxLength(10)
            .IsRequired();

        // Address → string
        builder.Property(s => s.SenderAddress)
            .HasConversion(a => a.Value, v => new Address(v))
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(s => s.RecipientAddress)
            .HasConversion(a => a.Value, v => new Address(v))
            .HasMaxLength(300)
            .IsRequired();

        // Nombres
        builder.Property(s => s.SenderName).HasMaxLength(150).IsRequired();
        builder.Property(s => s.RecipientName).HasMaxLength(150).IsRequired();

        // PackageDimensions → ComplexProperty (EF Core 8+): almacena inline sin identidad propia,
        // evita el problema de InMemory con OwnsOne que crea una "tabla" separada interna
        builder.ComplexProperty(s => s.Dimensions, d =>
        {
            d.Property(x => x.LengthCm).HasColumnName("LengthCm").HasColumnType("TEXT");
            d.Property(x => x.WidthCm).HasColumnName("WidthCm").HasColumnType("TEXT");
            d.Property(x => x.HeightCm).HasColumnName("HeightCm").HasColumnType("TEXT");
        });

        builder.Property(s => s.WeightKg).HasColumnType("TEXT");
        builder.Property(s => s.Fare).HasColumnType("TEXT");

        builder.Property(s => s.PackageType).IsRequired();
        builder.Property(s => s.ServiceType).IsRequired();
        builder.Property(s => s.OriginCity).IsRequired();
        builder.Property(s => s.DestinationCity).IsRequired();
        builder.Property(s => s.Status).IsRequired();
        builder.Property(s => s.CreatedAt).IsRequired();

        // EF Core necesita acceder al campo privado _statusHistory para materializar la colección
        // ya que StatusChanges expone IReadOnlyCollection (no es writable directamente)
        builder.HasMany(s => s.StatusChanges)
            .WithOne()
            .HasForeignKey(h => h.ShipmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(s => s.StatusChanges)
            .HasField("_statusHistory")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.ToTable("Shipments");
    }
}
