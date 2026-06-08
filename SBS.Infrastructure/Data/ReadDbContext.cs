using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SBS.Domain.Entities;
using SBS.Infrastructure.Identity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Infrastructure.Data;

public class ReadDbContext : IdentityDbContext<AppUser, AppRole, Guid>
{
    public ReadDbContext(DbContextOptions<ReadDbContext> options) : base(options)
    {
        // Disable tracking globally for optimized read operations
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    public DbSet<Pool> Pools { get; set; } = null!;
    public DbSet<PoolSlot> PoolSlots { get; set; } = null!;
    public DbSet<TicketType> TicketTypes { get; set; } = null!;
    public DbSet<ComboDetail> ComboDetails { get; set; } = null!;
    public DbSet<PoolTicketType> PoolTicketTypes { get; set; } = null!;
    public DbSet<Booking> Bookings { get; set; } = null!;
    public DbSet<BookingDetail> BookingDetails { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<CheckIn> CheckIns { get; set; } = null!;
    public DbSet<WaitlistEntry> WaitlistEntries { get; set; } = null!;
    public DbSet<ContactRequest> ContactRequests { get; set; } = null!;
    public DbSet<Feedback> Feedbacks { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    // Block any data modification on the read side to ensure read-only safety
    public override int SaveChanges()
    {
        throw new InvalidOperationException("ReadDbContext is read-only.");
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        throw new InvalidOperationException("ReadDbContext is read-only.");
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("ReadDbContext is read-only.");
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("ReadDbContext is read-only.");
    }
}
