using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SBS.Domain.Entities;
using SBS.Infrastructure.Identity;

namespace SBS.Infrastructure.Data.Configurations;

// ===== Pool =====
public class PoolConfiguration : IEntityTypeConfiguration<Pool>
{
    public void Configure(EntityTypeBuilder<Pool> builder)
    {
        builder.ToTable("Pools");
        builder.HasKey(e => e.PoolId);
        builder.Property(e => e.PoolId).ValueGeneratedOnAdd();
        builder.Property(e => e.PoolName).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Address).IsRequired().HasMaxLength(500);
        builder.Property(e => e.Description).HasMaxLength(2000);
        builder.Property(e => e.ImageUrl).HasMaxLength(500);
        builder.Property(e => e.OpeningTime).IsRequired().HasColumnType("time");
        builder.Property(e => e.ClosingTime).IsRequired().HasColumnType("time");
        builder.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Active");
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
    }
}

// ===== PoolSlot =====
public class PoolSlotConfiguration : IEntityTypeConfiguration<PoolSlot>
{
    public void Configure(EntityTypeBuilder<PoolSlot> builder)
    {
        builder.ToTable("PoolSlots");
        builder.HasKey(e => e.PoolSlotId);
        builder.Property(e => e.PoolSlotId).ValueGeneratedOnAdd();
        builder.Property(e => e.SlotName).HasMaxLength(100);
        builder.Property(e => e.StartTime).IsRequired().HasColumnType("time");
        builder.Property(e => e.EndTime).IsRequired().HasColumnType("time");
        builder.Property(e => e.SlotDate).IsRequired().HasColumnType("date");
        builder.Property(e => e.Capacity).IsRequired();
        builder.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Open");
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(e => e.Pool)
            .WithMany(p => p.PoolSlots)
            .HasForeignKey(e => e.PoolId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

// ===== TicketType =====
public class TicketTypeConfiguration : IEntityTypeConfiguration<TicketType>
{
    public void Configure(EntityTypeBuilder<TicketType> builder)
    {
        builder.ToTable("TicketTypes");
        builder.HasKey(e => e.TicketTypeId);
        builder.Property(e => e.TicketTypeId).ValueGeneratedOnAdd();
        builder.HasIndex(e => e.TicketCode).IsUnique();
        builder.HasIndex(e => e.TicketName).IsUnique();
        builder.Property(e => e.TicketCode).IsRequired().HasMaxLength(50);
        builder.Property(e => e.TicketName).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Category).IsRequired().HasMaxLength(10);
        builder.Property(e => e.BasePrice).HasColumnType("decimal(18,2)");
        builder.Property(e => e.DiscountPercent).HasColumnType("decimal(5,2)");
        builder.Property(e => e.Description).HasMaxLength(500);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Active");
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
    }
}

// ===== ComboDetail =====
public class ComboDetailConfiguration : IEntityTypeConfiguration<ComboDetail>
{
    public void Configure(EntityTypeBuilder<ComboDetail> builder)
    {
        builder.ToTable("ComboDetails");
        builder.HasKey(e => e.ComboDetailId);
        builder.Property(e => e.ComboDetailId).ValueGeneratedOnAdd();
        builder.Property(e => e.Quantity).IsRequired();

        builder.HasOne(e => e.ComboTicketType)
            .WithMany(t => t.ComboItems)
            .HasForeignKey(e => e.ComboTicketTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.SingleTicketType)
            .WithMany(t => t.IncludedInCombos)
            .HasForeignKey(e => e.SingleTicketTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

// ===== PoolTicketType =====
public class PoolTicketTypeConfiguration : IEntityTypeConfiguration<PoolTicketType>
{
    public void Configure(EntityTypeBuilder<PoolTicketType> builder)
    {
        builder.ToTable("PoolTicketTypes");
        builder.HasKey(e => e.PoolTicketTypeId);
        builder.Property(e => e.PoolTicketTypeId).ValueGeneratedOnAdd();
        builder.HasIndex(e => new { e.PoolId, e.TicketTypeId }).IsUnique();
        builder.Property(e => e.Price).HasColumnType("decimal(18,2)");
        builder.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Active");

        builder.HasOne(e => e.Pool)
            .WithMany(p => p.PoolTicketTypes)
            .HasForeignKey(e => e.PoolId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.TicketType)
            .WithMany(t => t.PoolTicketTypes)
            .HasForeignKey(e => e.TicketTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

// ===== Booking =====
public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");
        builder.HasKey(e => e.BookingId);
        builder.Property(e => e.BookingId).ValueGeneratedOnAdd();
        builder.HasIndex(e => e.BookingCode).IsUnique();
        builder.Property(e => e.BookingCode).IsRequired().HasMaxLength(20);
        builder.Property(e => e.UserId).IsRequired();
        builder.Property(e => e.BookingDate).IsRequired().HasColumnType("date");
        builder.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("PendingPayment");
        builder.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(e => e.QrCodeData).HasMaxLength(2000);
        builder.Property(e => e.BookingType).IsRequired().HasMaxLength(20).HasDefaultValue("Online");
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        // FK to AppUser (configured from Infrastructure side)
        builder.HasOne<AppUser>()
            .WithMany(u => u.Bookings)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.PoolSlot)
            .WithMany(ps => ps.Bookings)
            .HasForeignKey(e => e.PoolSlotId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

// ===== BookingDetail =====
public class BookingDetailConfiguration : IEntityTypeConfiguration<BookingDetail>
{
    public void Configure(EntityTypeBuilder<BookingDetail> builder)
    {
        builder.ToTable("BookingDetails");
        builder.HasKey(e => e.BookingDetailId);
        builder.Property(e => e.BookingDetailId).ValueGeneratedOnAdd();
        builder.Property(e => e.Quantity).IsRequired();
        builder.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Property(e => e.SubTotal).HasColumnType("decimal(18,2)");

        builder.HasOne(e => e.Booking)
            .WithMany(b => b.BookingDetails)
            .HasForeignKey(e => e.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.PoolTicketType)
            .WithMany(pt => pt.BookingDetails)
            .HasForeignKey(e => e.PoolTicketTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

// ===== Payment =====
public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(e => e.PaymentId);
        builder.Property(e => e.PaymentId).ValueGeneratedOnAdd();
        builder.HasIndex(e => e.BookingId).IsUnique(); // 1-to-1
        builder.Property(e => e.PaymentMethod).IsRequired().HasMaxLength(30);
        builder.Property(e => e.TransactionId).HasMaxLength(100);
        builder.Property(e => e.Amount).HasColumnType("decimal(18,2)");
        builder.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Pending");
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(e => e.Booking)
            .WithOne(b => b.Payment)
            .HasForeignKey<Payment>(e => e.BookingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

// ===== CheckIn =====
public class CheckInConfiguration : IEntityTypeConfiguration<CheckIn>
{
    public void Configure(EntityTypeBuilder<CheckIn> builder)
    {
        builder.ToTable("CheckIns");
        builder.HasKey(e => e.CheckInId);
        builder.Property(e => e.CheckInId).ValueGeneratedOnAdd();
        builder.HasIndex(e => e.BookingId).IsUnique(); // 1-to-1
        builder.Property(e => e.CheckedByUserId).IsRequired();
        builder.Property(e => e.CheckInMethod).IsRequired().HasMaxLength(20);
        builder.Property(e => e.CheckInTime).HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(e => e.Booking)
            .WithOne(b => b.CheckIn)
            .HasForeignKey<CheckIn>(e => e.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        // FK to AppUser (Staff who checked in)
        builder.HasOne<AppUser>()
            .WithMany(u => u.CheckIns)
            .HasForeignKey(e => e.CheckedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

// ===== WaitlistEntry =====
public class WaitlistEntryConfiguration : IEntityTypeConfiguration<WaitlistEntry>
{
    public void Configure(EntityTypeBuilder<WaitlistEntry> builder)
    {
        builder.ToTable("WaitlistEntries");
        builder.HasKey(e => e.WaitlistEntryId);
        builder.Property(e => e.WaitlistEntryId).ValueGeneratedOnAdd();
        builder.Property(e => e.UserId).IsRequired();
        builder.Property(e => e.Position).IsRequired();
        builder.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Waiting");
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        // FK to AppUser
        builder.HasOne<AppUser>()
            .WithMany(u => u.WaitlistEntries)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.PoolSlot)
            .WithMany(ps => ps.WaitlistEntries)
            .HasForeignKey(e => e.PoolSlotId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

// ===== ContactRequest =====
public class ContactRequestConfiguration : IEntityTypeConfiguration<ContactRequest>
{
    public void Configure(EntityTypeBuilder<ContactRequest> builder)
    {
        builder.ToTable("ContactRequests");
        builder.HasKey(e => e.ContactRequestId);
        builder.Property(e => e.ContactRequestId).ValueGeneratedOnAdd();
        builder.Property(e => e.Email).IsRequired().HasMaxLength(256);
        builder.Property(e => e.Reason).IsRequired().HasMaxLength(2000);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Pending");
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        // FK to AppUser (sender, nullable)
        builder.HasOne<AppUser>()
            .WithMany(u => u.ContactRequests)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // FK to AppUser (handler, nullable)
        builder.HasOne<AppUser>()
            .WithMany(u => u.HandledContacts)
            .HasForeignKey(e => e.HandledByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

// ===== Feedback =====
public class FeedbackConfiguration : IEntityTypeConfiguration<Feedback>
{
    public void Configure(EntityTypeBuilder<Feedback> builder)
    {
        builder.ToTable("Feedbacks");
        builder.HasKey(e => e.FeedbackId);
        builder.Property(e => e.FeedbackId).ValueGeneratedOnAdd();
        builder.Property(e => e.UserId).IsRequired();
        builder.Property(e => e.Rating).IsRequired();
        builder.Property(e => e.Comment).HasMaxLength(2000);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        // FK to AppUser
        builder.HasOne<AppUser>()
            .WithMany(u => u.Feedbacks)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Pool)
            .WithMany(p => p.Feedbacks)
            .HasForeignKey(e => e.PoolId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Booking)
            .WithOne(b => b.Feedback)
            .HasForeignKey<Feedback>(e => e.BookingId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
