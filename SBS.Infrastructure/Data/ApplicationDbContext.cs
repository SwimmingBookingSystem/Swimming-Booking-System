using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SBS.Domain.Entities;
using SBS.Infrastructure.Identity;
using System;

namespace SBS.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<AppUser, AppRole, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Pool> Pools { get; set; } = null!;
    public DbSet<PoolSlot> PoolSlots { get; set; } = null!;
    public DbSet<TicketType> TicketTypes { get; set; } = null!;
    public DbSet<TicketPriceHistory> TicketPriceHistories { get; set; } = null!;
    public DbSet<ComboDetail> ComboDetails { get; set; } = null!;
    public DbSet<PoolTicketType> PoolTicketTypes { get; set; } = null!;
    public DbSet<Booking> Bookings { get; set; } = null!;
    public DbSet<BookingDetail> BookingDetails { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<CheckIn> CheckIns { get; set; } = null!;
    public DbSet<WaitlistEntry> WaitlistEntries { get; set; } = null!;
    public DbSet<ContactRequest> ContactRequests { get; set; } = null!;
    public DbSet<Feedback> Feedbacks { get; set; } = null!;

    public DbSet<PoolImage> PoolImages { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<PoolStaffAssignment> PoolStaffAssignments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply all configurations defined in the assembly
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
