using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SBS.Infrastructure.Identity;

namespace SBS.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("user_id").ValueGeneratedOnAdd();
        builder.Property(u => u.UserName).HasColumnName("username").IsRequired().HasMaxLength(50);
        builder.HasIndex(u => u.UserName).IsUnique();
        
        builder.Property(u => u.PasswordHash).HasColumnName("password").IsRequired().HasMaxLength(255);
        builder.Property(u => u.FullName).HasColumnName("full_name").IsRequired().HasMaxLength(100);
        builder.Property(u => u.Email).HasColumnName("email").IsRequired().HasMaxLength(100);
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.PhoneNumber).HasColumnName("phone").HasMaxLength(15);
        builder.Property(u => u.Address).HasColumnName("address").HasMaxLength(255);
        builder.Property(u => u.RoleId).HasColumnName("role_id").IsRequired();
        builder.Property(u => u.Status).HasColumnName("status").HasDefaultValue(true);
        builder.Property(u => u.Dob).HasColumnName("dob");
        builder.Property(u => u.Gender).HasColumnName("gender").HasMaxLength(10);
        builder.Property(u => u.Images).HasColumnName("images").HasMaxLength(255);
        builder.Property(u => u.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETDATE()");
        builder.Property(u => u.UpdatedAt).HasColumnName("updated_at");

        // Single Role relationship
        builder.HasOne<AppRole>()
            .WithMany()
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class RoleConfiguration : IEntityTypeConfiguration<AppRole>
{
    public void Configure(EntityTypeBuilder<AppRole> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("role_id").ValueGeneratedOnAdd();
        builder.Property(r => r.Name).HasColumnName("role_name").IsRequired().HasMaxLength(50);
        builder.HasIndex(r => r.Name).IsUnique();
    }
}
