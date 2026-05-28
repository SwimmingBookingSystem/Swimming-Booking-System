using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SBS.Domain.Entities;
using SBS.Infrastructure.Identity;

namespace SBS.Infrastructure.Data.Configurations;

public class PoolConfiguration : IEntityTypeConfiguration<Pool>
{
    public void Configure(EntityTypeBuilder<Pool> builder)
    {
        builder.ToTable("Pools");

        builder.HasKey(p => p.PoolId);
        builder.Property(p => p.PoolId).HasColumnName("pool_id").ValueGeneratedOnAdd();
        builder.Property(p => p.PoolName).HasColumnName("pool_name").IsRequired().HasMaxLength(100);
        builder.Property(p => p.PoolRoad).HasColumnName("pool_road").IsRequired().HasMaxLength(255);
        builder.Property(p => p.PoolAddress).HasColumnName("pool_address").IsRequired().HasMaxLength(255);
        builder.Property(p => p.MaxSlot).HasColumnName("max_slot").IsRequired();
        builder.Property(p => p.OpenTime).HasColumnName("open_time").IsRequired();
        builder.Property(p => p.CloseTime).HasColumnName("close_time").IsRequired();
        builder.Property(p => p.PoolStatus).HasColumnName("pool_status").HasDefaultValue(true);
        builder.Property(p => p.PoolImage).HasColumnName("pool_image").HasMaxLength(255);
        builder.Property(p => p.PoolDescription).HasColumnName("pool_description").HasMaxLength(255);
        builder.Property(p => p.BranchId).HasColumnName("branch_id").IsRequired();
        builder.Property(p => p.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETDATE()");
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at");

        // Relationships
        builder.HasOne(p => p.Branch)
            .WithMany(b => b.Pools)
            .HasForeignKey(p => p.BranchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PoolImageConfiguration : IEntityTypeConfiguration<PoolImage>
{
    public void Configure(EntityTypeBuilder<PoolImage> builder)
    {
        builder.ToTable("PoolImage");

        builder.HasKey(pi => pi.ImageId);
        builder.Property(pi => pi.ImageId).HasColumnName("image_id").ValueGeneratedOnAdd();
        builder.Property(pi => pi.PoolId).HasColumnName("pool_id").IsRequired();
        builder.Property(pi => pi.PoolImageLink).HasColumnName("pool_image").IsRequired();

        // Relationship
        builder.HasOne(pi => pi.Pool)
            .WithMany(p => p.PoolImages)
            .HasForeignKey(pi => pi.PoolId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PoolDeviceConfiguration : IEntityTypeConfiguration<PoolDevice>
{
    public void Configure(EntityTypeBuilder<PoolDevice> builder)
    {
        builder.ToTable("Pool_Device");

        builder.HasKey(pd => pd.DeviceId);
        builder.Property(pd => pd.DeviceId).HasColumnName("device_id").ValueGeneratedOnAdd();
        builder.Property(pd => pd.PoolId).HasColumnName("pool_id").IsRequired();
        builder.Property(pd => pd.DeviceImage).HasColumnName("device_image").HasMaxLength(255);
        builder.Property(pd => pd.DeviceName).HasColumnName("device_name").IsRequired().HasMaxLength(100);
        builder.Property(pd => pd.Quantity).HasColumnName("quantity").IsRequired();
        builder.Property(pd => pd.DeviceStatus).HasColumnName("device_status").IsRequired().HasMaxLength(50);
        builder.Property(pd => pd.Notes).HasColumnName("notes").HasMaxLength(255);

        // Relationship
        builder.HasOne(pd => pd.Pool)
            .WithMany(p => p.PoolDevices)
            .HasForeignKey(pd => pd.PoolId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class StaffTypeConfiguration : IEntityTypeConfiguration<StaffType>
{
    public void Configure(EntityTypeBuilder<StaffType> builder)
    {
        builder.ToTable("Staff_Types");

        builder.HasKey(st => st.StaffTypeId);
        builder.Property(st => st.StaffTypeId).HasColumnName("staff_type_id").ValueGeneratedOnAdd();
        builder.Property(st => st.TypeName).HasColumnName("type_name").IsRequired().HasMaxLength(50);
        builder.Property(st => st.Description).HasColumnName("description").HasMaxLength(255);
    }
}

public class StaffConfiguration : IEntityTypeConfiguration<Staff>
{
    public void Configure(EntityTypeBuilder<Staff> builder)
    {
        builder.ToTable("Staffs");

        builder.HasKey(s => s.StaffId);
        builder.Property(s => s.StaffId).HasColumnName("staff_id").ValueGeneratedOnAdd();
        builder.Property(s => s.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(s => s.BranchId).HasColumnName("branch_id").IsRequired();
        builder.Property(s => s.PoolId).HasColumnName("pool_id").IsRequired();
        builder.Property(s => s.StaffTypeId).HasColumnName("staff_type_id").IsRequired();

        // Relationships
        builder.HasOne(s => s.Branch)
            .WithMany(b => b.Staffs)
            .HasForeignKey(s => s.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Pool)
            .WithMany(p => p.Staffs)
            .HasForeignKey(s => s.PoolId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.StaffType)
            .WithMany(st => st.Staffs)
            .HasForeignKey(s => s.StaffTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
