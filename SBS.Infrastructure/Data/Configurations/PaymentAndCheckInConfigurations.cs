using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SBS.Domain.Entities;

namespace SBS.Infrastructure.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Amount).HasColumnType("decimal(18,2)");
        builder.Property(p => p.PaymentMethod).HasConversion<int>();
        builder.Property(p => p.Status).HasConversion<int>();

        builder.HasOne(p => p.Booking)
            .WithOne(b => b.Payment)
            .HasForeignKey<Payment>(p => p.BookingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CheckInConfiguration : IEntityTypeConfiguration<CheckIn>
{
    public void Configure(EntityTypeBuilder<CheckIn> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Status).HasConversion<int>();

        builder.HasOne(c => c.Booking)
            .WithOne(b => b.CheckIn)
            .HasForeignKey<CheckIn>(c => c.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Receptionist)
            .WithMany(u => u.CheckInsAsReceptionist)
            .HasForeignKey(c => c.ReceptionistId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
