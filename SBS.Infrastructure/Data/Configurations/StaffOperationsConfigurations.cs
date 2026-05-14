using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SBS.Domain.Entities;

namespace SBS.Infrastructure.Data.Configurations;

public class MaintenanceRequestConfiguration : IEntityTypeConfiguration<MaintenanceRequest>
{
    public void Configure(EntityTypeBuilder<MaintenanceRequest> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Description).IsRequired().HasMaxLength(500);
        builder.Property(m => m.Status).HasConversion<int>();

        builder.HasOne(m => m.Pool)
            .WithMany(p => p.MaintenanceRequests)
            .HasForeignKey(m => m.PoolId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.RequestedBy)
            .WithMany(u => u.MaintenanceRequests)
            .HasForeignKey(m => m.RequestedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class StaffShiftConfiguration : IEntityTypeConfiguration<StaffShift>
{
    public void Configure(EntityTypeBuilder<StaffShift> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Area).HasMaxLength(100);

        builder.HasOne(s => s.Staff)
            .WithMany(u => u.Shifts)
            .HasForeignKey(s => s.StaffId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
