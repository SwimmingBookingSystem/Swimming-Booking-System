using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SBS.Application.Common.Interfaces;
using SBS.Domain.Entities;
using SBS.Infrastructure.Identity;

namespace SBS.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<AppUser, AppRole, int>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Branch> Branchs { get; set; } = null!;
    public DbSet<Pool> Pools { get; set; } = null!;
    public DbSet<StaffType> StaffTypes { get; set; } = null!;
    public DbSet<Staff> Staffs { get; set; } = null!;
    public DbSet<PoolDevice> PoolDevices { get; set; } = null!;
    public DbSet<Discount> Discounts { get; set; } = null!;
    public DbSet<Booking> Bookings { get; set; } = null!;
    public DbSet<PoolService> PoolServices { get; set; } = null!;
    public DbSet<BookingService> BookingServices { get; set; } = null!;
    public DbSet<Feedback> Feedbacks { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<TicketType> TicketTypes { get; set; } = null!;
    public DbSet<ComboDetail> ComboDetails { get; set; } = null!;
    public DbSet<PoolTicketType> PoolTicketTypes { get; set; } = null!;
    public DbSet<Ticket> Tickets { get; set; } = null!;
    public DbSet<Contact> Contacts { get; set; } = null!;
    public DbSet<ContactResponse> ContactResponses { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<DiscountAuditLog> DiscountAuditLogs { get; set; } = null!;
    public DbSet<ServiceReport> ServiceReports { get; set; } = null!;
    public DbSet<DeviceReport> DeviceReports { get; set; } = null!;
    public DbSet<CustomerCheckin> CustomerCheckins { get; set; } = null!;
    public DbSet<SaleTicketDirectly> SaleTicketDirectlys { get; set; } = null!;
    public DbSet<AccountBanLog> AccountBanLogs { get; set; } = null!;
    public DbSet<PoolImage> PoolImages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply all configurations defined in the assembly
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
