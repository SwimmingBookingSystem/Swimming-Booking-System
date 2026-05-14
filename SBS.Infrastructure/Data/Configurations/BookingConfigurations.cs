using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SBS.Domain.Entities;

namespace SBS.Infrastructure.Data.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.TotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(b => b.Status).HasConversion<int>();

        builder.HasOne(b => b.Customer)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class BookingDetailConfiguration : IEntityTypeConfiguration<BookingDetail>
{
    public void Configure(EntityTypeBuilder<BookingDetail> builder)
    {
        builder.HasKey(bd => bd.Id);
        builder.Property(bd => bd.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Property(bd => bd.SubTotal).HasColumnType("decimal(18,2)");

        builder.HasOne(bd => bd.Booking)
            .WithMany(b => b.BookingDetails)
            .HasForeignKey(bd => bd.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(bd => bd.Schedule)
            .WithMany()
            .HasForeignKey(bd => bd.ScheduleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
