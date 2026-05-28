using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SBS.Domain.Entities;
using SBS.Infrastructure.Identity;

namespace SBS.Infrastructure.Data.Configurations;

public class FeedbackConfiguration : IEntityTypeConfiguration<Feedback>
{
    public void Configure(EntityTypeBuilder<Feedback> builder)
    {
        builder.ToTable("Feedback");

        builder.HasKey(f => f.FeedbackId);
        builder.Property(f => f.FeedbackId).HasColumnName("feedback_id").ValueGeneratedOnAdd();
        builder.Property(f => f.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(f => f.PoolId).HasColumnName("pool_id").IsRequired();
        builder.Property(f => f.BookingId).HasColumnName("booking_id").IsRequired();
        builder.Property(f => f.Rating).HasColumnName("rating").IsRequired();
        builder.Property(f => f.Comment).HasColumnName("comment").HasMaxLength(1000);
        builder.Property(f => f.Replied).HasColumnName("replied");
        builder.Property(f => f.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETDATE()");

        // Relationships
        builder.HasOne(f => f.Booking)
            .WithMany(b => b.Feedbacks)
            .HasForeignKey(f => f.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.Pool)
            .WithMany(p => p.Feedbacks)
            .HasForeignKey(f => f.PoolId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.ToTable("Contact");

        builder.HasKey(c => c.ContactId);
        builder.Property(c => c.ContactId).HasColumnName("contact_id").ValueGeneratedOnAdd();
        builder.Property(c => c.UserId).HasColumnName("user_id");
        builder.Property(c => c.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
        builder.Property(c => c.Email).HasColumnName("email").IsRequired().HasMaxLength(100);
        builder.Property(c => c.Subject).HasColumnName("subject").IsRequired().HasMaxLength(255);
        builder.Property(c => c.Content).HasColumnName("content").IsRequired();
        builder.Property(c => c.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETDATE()");
        builder.Property(c => c.IsResolved).HasColumnName("is_resolved").HasDefaultValue(false);

        // Relationships
        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class ContactResponseConfiguration : IEntityTypeConfiguration<ContactResponse>
{
    public void Configure(EntityTypeBuilder<ContactResponse> builder)
    {
        builder.ToTable("ContactResponse");

        builder.HasKey(cr => cr.ResponseId);
        builder.Property(cr => cr.ResponseId).HasColumnName("response_id").ValueGeneratedOnAdd();
        builder.Property(cr => cr.ContactId).HasColumnName("contact_id").IsRequired();
        builder.Property(cr => cr.ResponderId).HasColumnName("responder_id").IsRequired();
        builder.Property(cr => cr.ResponseContent).HasColumnName("response_content").IsRequired();
        builder.Property(cr => cr.ResponseTime).HasColumnName("response_time").HasDefaultValueSql("GETDATE()");

        // Relationships
        builder.HasOne(cr => cr.Contact)
            .WithMany(c => c.ContactResponses)
            .HasForeignKey(cr => cr.ContactId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(cr => cr.ResponderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notification");

        builder.HasKey(n => n.NotificationId);
        builder.Property(n => n.NotificationId).HasColumnName("notification_id").ValueGeneratedOnAdd();
        builder.Property(n => n.Title).HasColumnName("title").IsRequired().HasMaxLength(100);
        builder.Property(n => n.Content).HasColumnName("content").IsRequired();
        builder.Property(n => n.CreatedBy).HasColumnName("created_by").IsRequired();
        builder.Property(n => n.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETDATE()");
        builder.Property(n => n.TargetRoleId).HasColumnName("target_role_id");
        builder.Property(n => n.TargetBranchId).HasColumnName("target_branch_id");

        // Relationships
        builder.HasOne(n => n.Branch)
            .WithMany()
            .HasForeignKey(n => n.TargetBranchId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<AppRole>()
            .WithMany()
            .HasForeignKey(n => n.TargetRoleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(n => n.CreatedBy)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class DiscountAuditLogConfiguration : IEntityTypeConfiguration<DiscountAuditLog>
{
    public void Configure(EntityTypeBuilder<DiscountAuditLog> builder)
    {
        builder.ToTable("DiscountAuditLog");

        builder.HasKey(dal => dal.LogId);
        builder.Property(dal => dal.LogId).HasColumnName("log_id").ValueGeneratedOnAdd();
        builder.Property(dal => dal.DiscountId).HasColumnName("discount_id").IsRequired();
        builder.Property(dal => dal.ManagerId).HasColumnName("manager_id").IsRequired();
        builder.Property(dal => dal.ActionType).HasColumnName("action_type").IsRequired().HasMaxLength(50);
        builder.Property(dal => dal.ActionTime).HasColumnName("action_time").HasDefaultValueSql("GETDATE()");
        builder.Property(dal => dal.OldValues).HasColumnName("old_values");
        builder.Property(dal => dal.NewValues).HasColumnName("new_values");
        builder.Property(dal => dal.Notes).HasColumnName("notes").HasMaxLength(255);

        // Relationships
        builder.HasOne(dal => dal.Discount)
            .WithMany(d => d.DiscountAuditLogs)
            .HasForeignKey(dal => dal.DiscountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(dal => dal.ManagerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ServiceReportConfiguration : IEntityTypeConfiguration<ServiceReport>
{
    public void Configure(EntityTypeBuilder<ServiceReport> builder)
    {
        builder.ToTable("ServiceReport");

        builder.HasKey(sr => sr.ReportId);
        builder.Property(sr => sr.ReportId).HasColumnName("report_id").ValueGeneratedOnAdd();
        builder.Property(sr => sr.StaffId).HasColumnName("staff_id").IsRequired();
        builder.Property(sr => sr.ServiceId).HasColumnName("service_id").IsRequired();
        builder.Property(sr => sr.ReportReason).HasColumnName("report_reason").IsRequired().HasMaxLength(255);
        builder.Property(sr => sr.Suggestion).HasColumnName("suggestion").HasMaxLength(255);
        builder.Property(sr => sr.ReportDate).HasColumnName("report_date").HasDefaultValueSql("GETDATE()");
        builder.Property(sr => sr.Status).HasColumnName("status").IsRequired().HasMaxLength(50);
        builder.Property(sr => sr.ManagerNote).HasColumnName("manager_note").HasMaxLength(255);
        builder.Property(sr => sr.ProcessedAt).HasColumnName("processed_at");
        builder.Property(sr => sr.ProcessedBy).HasColumnName("processed_by");

        // Relationships
        builder.HasOne(sr => sr.Staff)
            .WithMany(s => s.ServiceReports)
            .HasForeignKey(sr => sr.StaffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sr => sr.PoolService)
            .WithMany(ps => ps.ServiceReports)
            .HasForeignKey(sr => sr.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(sr => sr.ProcessedBy)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class DeviceReportConfiguration : IEntityTypeConfiguration<DeviceReport>
{
    public void Configure(EntityTypeBuilder<DeviceReport> builder)
    {
        builder.ToTable("DeviceReport");

        builder.HasKey(dr => dr.ReportId);
        builder.Property(dr => dr.ReportId).HasColumnName("report_id").ValueGeneratedOnAdd();
        builder.Property(dr => dr.StaffId).HasColumnName("staff_id").IsRequired();
        builder.Property(dr => dr.DeviceId).HasColumnName("device_id");
        builder.Property(dr => dr.ReportReason).HasColumnName("report_reason").IsRequired().HasMaxLength(255);
        builder.Property(dr => dr.Suggestion).HasColumnName("suggestion").HasMaxLength(255);
        builder.Property(dr => dr.ReportDate).HasColumnName("report_date").HasDefaultValueSql("GETDATE()");
        builder.Property(dr => dr.Status).HasColumnName("status").IsRequired().HasMaxLength(50);
        builder.Property(dr => dr.ManagerNote).HasColumnName("manager_note").HasMaxLength(255);
        builder.Property(dr => dr.ProcessedAt).HasColumnName("processed_at");
        builder.Property(dr => dr.ProcessedBy).HasColumnName("processed_by");

        // Relationships
        builder.HasOne(dr => dr.Staff)
            .WithMany(s => s.DeviceReports)
            .HasForeignKey(dr => dr.StaffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(dr => dr.PoolDevice)
            .WithMany(pd => pd.DeviceReports)
            .HasForeignKey(dr => dr.DeviceId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(dr => dr.ProcessedBy)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class SaleTicketDirectlyConfiguration : IEntityTypeConfiguration<SaleTicketDirectly>
{
    public void Configure(EntityTypeBuilder<SaleTicketDirectly> builder)
    {
        builder.ToTable("SaleTicketDirectly");

        builder.HasKey(std => std.SaleId);
        builder.Property(std => std.SaleId).HasColumnName("sale_id").ValueGeneratedOnAdd();
        builder.Property(std => std.CustomerName).HasColumnName("customer_name").HasMaxLength(100);
        builder.Property(std => std.CustomerPhone).HasColumnName("customer_phone").HasMaxLength(15);
        builder.Property(std => std.CustomerEmail).HasColumnName("customer_email").HasMaxLength(100);
        builder.Property(std => std.UserId).HasColumnName("user_id");
        builder.Property(std => std.StaffId).HasColumnName("staff_id").IsRequired();
        builder.Property(std => std.BookingId).HasColumnName("booking_id").IsRequired();
        builder.Property(std => std.TotalAmount).HasColumnName("total_amount").HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(std => std.PaymentMethod).HasColumnName("payment_method").IsRequired().HasMaxLength(50);
        builder.Property(std => std.PaymentStatus).HasColumnName("payment_status").IsRequired().HasMaxLength(50);
        builder.Property(std => std.SaleDate).HasColumnName("sale_date").HasDefaultValueSql("GETDATE()");
        builder.Property(std => std.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETDATE()");
        builder.Property(std => std.Notes).HasColumnName("notes").HasMaxLength(255);

        // Relationships
        builder.HasOne(std => std.Staff)
            .WithMany(s => s.SaleTicketDirectlys)
            .HasForeignKey(std => std.StaffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(std => std.Booking)
            .WithMany(b => b.SaleTicketDirectlys)
            .HasForeignKey(std => std.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(std => std.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class AccountBanLogConfiguration : IEntityTypeConfiguration<AccountBanLog>
{
    public void Configure(EntityTypeBuilder<AccountBanLog> builder)
    {
        builder.ToTable("Account_Ban_Log");

        builder.HasKey(abl => abl.BanId);
        builder.Property(abl => abl.BanId).HasColumnName("ban_id").ValueGeneratedOnAdd();
        builder.Property(abl => abl.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(abl => abl.BannedBy).HasColumnName("banned_by").IsRequired();
        builder.Property(abl => abl.Reason).HasColumnName("reason").IsRequired().HasMaxLength(255);
        builder.Property(abl => abl.IsPermanent).HasColumnName("is_permanent").HasDefaultValue(true);
        builder.Property(abl => abl.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETDATE()");

        // Relationships
        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(abl => abl.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(abl => abl.BannedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
