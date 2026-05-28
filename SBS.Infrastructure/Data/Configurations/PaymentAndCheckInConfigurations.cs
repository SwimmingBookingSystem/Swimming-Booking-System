using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SBS.Domain.Entities;
using SBS.Infrastructure.Identity;

namespace SBS.Infrastructure.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payment");

        builder.HasKey(p => p.PaymentId);
        builder.Property(p => p.PaymentId).HasColumnName("payment_id").ValueGeneratedOnAdd();
        builder.Property(p => p.BookingId).HasColumnName("booking_id").IsRequired();
        builder.Property(p => p.PaymentMethod).HasColumnName("payment_method").IsRequired().HasMaxLength(50);
        builder.Property(p => p.PaymentStatus).HasColumnName("payment_status").IsRequired().HasMaxLength(50);
        builder.Property(p => p.PaymentDate).HasColumnName("payment_date");
        builder.Property(p => p.TotalAmount).HasColumnName("total_amount").HasColumnType("decimal(18,2)");
        builder.Property(p => p.DiscountAmount).HasColumnName("discount_amount").HasColumnType("decimal(18,2)");
        builder.Property(p => p.TransactionReference).HasColumnName("transaction_reference").HasMaxLength(100);
        builder.Property(p => p.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETDATE()");

        // Relationships
        builder.HasOne(p => p.Booking)
            .WithMany(b => b.Payments)
            .HasForeignKey(p => p.BookingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CustomerCheckinConfiguration : IEntityTypeConfiguration<CustomerCheckin>
{
    public void Configure(EntityTypeBuilder<CustomerCheckin> builder)
    {
        builder.ToTable("CustomerCheckin");

        builder.HasKey(c => c.CheckinId);
        builder.Property(c => c.CheckinId).HasColumnName("checkin_id").ValueGeneratedOnAdd();
        builder.Property(c => c.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(c => c.BookingId).HasColumnName("booking_id").IsRequired();
        builder.Property(c => c.CheckinStatus).HasColumnName("checkin_status");
        builder.Property(c => c.CheckinTime).HasColumnName("checkin_time").HasDefaultValueSql("GETDATE()");

        // Relationships
        builder.HasOne(c => c.Booking)
            .WithMany(b => b.CustomerCheckins)
            .HasForeignKey(c => c.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
