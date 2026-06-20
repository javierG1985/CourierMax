using Ceiba.CourierMax.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ceiba.CourierMax.Persistence.Configurations;

public class StatusHistoryConfiguration : IEntityTypeConfiguration<StatusHistory>
{
    public void Configure(EntityTypeBuilder<StatusHistory> builder)
    {
        builder.HasKey(h => h.Id);
        builder.Property(h => h.ShipmentId).IsRequired();
        builder.Property(h => h.PreviousStatus).IsRequired();
        builder.Property(h => h.NewStatus).IsRequired();
        builder.Property(h => h.ChangedAt).IsRequired();
        builder.Property(h => h.ChangedBy).HasMaxLength(100).IsRequired();
        builder.Property(h => h.Reason).HasMaxLength(500);

        builder.ToTable("StatusHistories");
    }
}
