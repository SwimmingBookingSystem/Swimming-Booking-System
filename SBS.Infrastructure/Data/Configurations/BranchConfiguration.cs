using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SBS.Domain.Entities;
using SBS.Infrastructure.Identity;

namespace SBS.Infrastructure.Data.Configurations;

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("Branchs");

        builder.HasKey(b => b.BranchId);
        builder.Property(b => b.BranchId).HasColumnName("branch_id").ValueGeneratedOnAdd();
        builder.Property(b => b.BranchName).HasColumnName("branch_name").IsRequired().HasMaxLength(100);
        builder.Property(b => b.ManagerId).HasColumnName("manager_id");

        // Unique FK constraint to AppUser
        builder.HasIndex(b => b.ManagerId).IsUnique();
        
        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(b => b.ManagerId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
