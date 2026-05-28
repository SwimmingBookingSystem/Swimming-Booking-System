using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SBS.Domain.Entities;
using SBS.Infrastructure.Identity;

namespace SBS.Infrastructure.Data.Configurations;

public class DiscountConfiguration : IEntityTypeConfiguration<Discount>
{
    public void Configure(EntityTypeBuilder<Discount> builder)
    {
        builder.ToTable("Discount");

        builder.HasKey(d => d.DiscountId);
        builder.Property(d => d.DiscountId).HasColumnName("discount_id").ValueGeneratedOnAdd();
        builder.Property(d => d.DiscountCode).HasColumnName("discount_code").IsRequired().HasMaxLength(50);
        builder.Property(d => d.Description).HasColumnName("description").HasMaxLength(255);
        builder.Property(d => d.DiscountPercent).HasColumnName("discount_percent").HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(d => d.Quantity).HasColumnName("quantity");
        builder.Property(d => d.ValidFrom).HasColumnName("valid_from").IsRequired();
        builder.Property(d => d.ValidTo).HasColumnName("valid_to").IsRequired();
        builder.Property(d => d.Status).HasColumnName("status").HasDefaultValue(true);
        builder.Property(d => d.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETDATE()");
        builder.Property(d => d.UpdatedAt).HasColumnName("updated_at");
        builder.Property(d => d.CreatedBy).HasColumnName("created_by").IsRequired();

        // Relationship
        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(d => d.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Booking");

        builder.HasKey(b => b.BookingId);
        builder.Property(b => b.BookingId).HasColumnName("booking_id").ValueGeneratedOnAdd();
        builder.Property(b => b.UserId).HasColumnName("user_id");
        builder.Property(b => b.PoolId).HasColumnName("pool_id").IsRequired();
        builder.Property(b => b.DiscountId).HasColumnName("discount_id");
        builder.Property(b => b.BookingDate).HasColumnName("booking_date").IsRequired();
        builder.Property(b => b.StartTime).HasColumnName("start_time").IsRequired();
        builder.Property(b => b.EndTime).HasColumnName("end_time").IsRequired();
        builder.Property(b => b.SlotCount).HasColumnName("slot_count").IsRequired();
        builder.Property(b => b.BookingStatus).HasColumnName("booking_status").IsRequired().HasMaxLength(50);
        builder.Property(b => b.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETDATE()");
        builder.Property(b => b.UpdatedAt).HasColumnName("updated_at");

        // Relationships
        builder.HasOne(b => b.Pool)
            .WithMany(p => p.Bookings)
            .HasForeignKey(b => b.PoolId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(b => b.Discount)
            .WithMany(d => d.Bookings)
            .HasForeignKey(b => b.DiscountId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class PoolServiceConfiguration : IEntityTypeConfiguration<PoolService>
{
    public void Configure(EntityTypeBuilder<PoolService> builder)
    {
        builder.ToTable("PoolService");

        builder.HasKey(ps => ps.PoolServiceId);
        builder.Property(ps => ps.PoolServiceId).HasColumnName("pool_service_id").ValueGeneratedOnAdd();
        builder.Property(ps => ps.PoolId).HasColumnName("pool_id").IsRequired();
        builder.Property(ps => ps.ServiceName).HasColumnName("service_name").IsRequired().HasMaxLength(100);
        builder.Property(ps => ps.Description).HasColumnName("description").HasMaxLength(255);
        builder.Property(ps => ps.Price).HasColumnName("price").HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(ps => ps.ServiceImage).HasColumnName("service_image").HasMaxLength(255);
        builder.Property(ps => ps.Quantity).HasColumnName("quantity").IsRequired();
        builder.Property(ps => ps.ServiceStatus).HasColumnName("service_status").IsRequired().HasMaxLength(50);

        // Relationship
        builder.HasOne(ps => ps.Pool)
            .WithMany(p => p.PoolServices)
            .HasForeignKey(ps => ps.PoolId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class BookingServiceConfiguration : IEntityTypeConfiguration<BookingService>
{
    public void Configure(EntityTypeBuilder<BookingService> builder)
    {
        builder.ToTable("BookingService");

        builder.HasKey(bs => bs.BookingServiceId);
        builder.Property(bs => bs.BookingServiceId).HasColumnName("booking_service_id").ValueGeneratedOnAdd();
        builder.Property(bs => bs.BookingId).HasColumnName("booking_id").IsRequired();
        builder.Property(bs => bs.PoolServiceId).HasColumnName("pool_service_id").IsRequired();
        builder.Property(bs => bs.Quantity).HasColumnName("quantity");
        builder.Property(bs => bs.TotalServicePrice).HasColumnName("total_service_price").HasColumnType("decimal(18,2)").IsRequired();

        // Relationships
        builder.HasOne(bs => bs.Booking)
            .WithMany(b => b.BookingServices)
            .HasForeignKey(bs => bs.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(bs => bs.PoolService)
            .WithMany(ps => ps.BookingServices)
            .HasForeignKey(bs => bs.PoolServiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class TicketTypeConfiguration : IEntityTypeConfiguration<TicketType>
{
    public void Configure(EntityTypeBuilder<TicketType> builder)
    {
        builder.ToTable("TicketType");

        builder.HasKey(tt => tt.TicketTypeId);
        builder.Property(tt => tt.TicketTypeId).HasColumnName("ticket_type_id").ValueGeneratedOnAdd();
        builder.Property(tt => tt.TypeCode).HasColumnName("type_code").IsRequired().HasMaxLength(50);
        builder.Property(tt => tt.TypeName).HasColumnName("type_name").IsRequired().HasMaxLength(100);
        builder.Property(tt => tt.Description).HasColumnName("description").HasMaxLength(255);
        builder.Property(tt => tt.BasePrice).HasColumnName("base_price").HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(tt => tt.IsCombo).HasColumnName("is_combo").HasDefaultValue(false);
        builder.Property(tt => tt.DiscountPercent).HasColumnName("discount_percent").HasColumnType("decimal(18,2)");
        builder.Property(tt => tt.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETDATE()");
    }
}

public class ComboDetailConfiguration : IEntityTypeConfiguration<ComboDetail>
{
    public void Configure(EntityTypeBuilder<ComboDetail> builder)
    {
        builder.ToTable("ComboDetail");

        builder.HasKey(cd => new { cd.ComboTypeId, cd.IncludedTypeId });
        builder.Property(cd => cd.ComboTypeId).HasColumnName("combo_type_id").IsRequired();
        builder.Property(cd => cd.IncludedTypeId).HasColumnName("included_type_id").IsRequired();
        builder.Property(cd => cd.Quantity).HasColumnName("quantity").IsRequired();

        // Relationships
        builder.HasOne(cd => cd.ComboType)
            .WithMany(t => t.ComboDetails)
            .HasForeignKey(cd => cd.ComboTypeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cd => cd.IncludedType)
            .WithMany()
            .HasForeignKey(cd => cd.IncludedTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PoolTicketTypeConfiguration : IEntityTypeConfiguration<PoolTicketType>
{
    public void Configure(EntityTypeBuilder<PoolTicketType> builder)
    {
        builder.ToTable("PoolTicketType");

        builder.HasKey(pt => new { pt.PoolId, pt.TicketTypeId });
        builder.Property(pt => pt.PoolId).HasColumnName("pool_id").IsRequired();
        builder.Property(pt => pt.TicketTypeId).HasColumnName("ticket_type_id").IsRequired();
        builder.Property(pt => pt.Status).HasColumnName("status").IsRequired().HasMaxLength(50);

        // Relationships
        builder.HasOne(pt => pt.Pool)
            .WithMany(p => p.PoolTicketTypes)
            .HasForeignKey(pt => pt.PoolId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pt => pt.TicketType)
            .WithMany(tt => tt.PoolTicketTypes)
            .HasForeignKey(pt => pt.TicketTypeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("Ticket");

        builder.HasKey(t => t.TicketId);
        builder.Property(t => t.TicketId).HasColumnName("ticket_id").ValueGeneratedOnAdd();
        builder.Property(t => t.BookingId).HasColumnName("booking_id").IsRequired();
        builder.Property(t => t.TicketTypeId).HasColumnName("ticket_type_id").IsRequired();
        builder.Property(t => t.TicketPrice).HasColumnName("ticket_price").HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(t => t.TicketCode).HasColumnName("ticket_code").IsRequired().HasMaxLength(50);
        builder.Property(t => t.IssuedBy).HasColumnName("issued_by");
        builder.Property(t => t.IssuedAt).HasColumnName("issued_at");

        // Relationships
        builder.HasOne(t => t.Booking)
            .WithMany(b => b.Tickets)
            .HasForeignKey(t => t.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.TicketType)
            .WithMany(tt => tt.Tickets)
            .HasForeignKey(t => t.TicketTypeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(t => t.IssuedBy)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
