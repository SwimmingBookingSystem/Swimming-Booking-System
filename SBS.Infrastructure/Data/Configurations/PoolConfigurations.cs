using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SBS.Domain.Entities;

namespace SBS.Infrastructure.Data.Configurations;

public class SwimmingPoolConfiguration : IEntityTypeConfiguration<SwimmingPool>
{
    public void Configure(EntityTypeBuilder<SwimmingPool> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Location).HasMaxLength(200);
        builder.Property(p => p.Dimension).HasMaxLength(50);
        builder.Property(p => p.Status).HasConversion<int>();
    }
}

public class PoolImageConfiguration : IEntityTypeConfiguration<PoolImage>
{
    public void Configure(EntityTypeBuilder<PoolImage> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.ImageUrl).IsRequired().HasMaxLength(500);
        builder.Property(i => i.Caption).HasMaxLength(200);

        builder.HasOne(i => i.Pool)
            .WithMany(p => p.Images)
            .HasForeignKey(i => i.PoolId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PoolScheduleConfiguration : IEntityTypeConfiguration<PoolSchedule>
{
    public void Configure(EntityTypeBuilder<PoolSchedule> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Price).HasColumnType("decimal(18,2)");

        builder.HasOne(s => s.Pool)
            .WithMany(p => p.Schedules)
            .HasForeignKey(s => s.PoolId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
