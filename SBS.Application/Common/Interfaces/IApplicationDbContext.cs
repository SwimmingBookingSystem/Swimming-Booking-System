using Microsoft.EntityFrameworkCore;
using SBS.Domain.Entities;

namespace SBS.Application.Common.Interfaces;

/// <summary>
/// Abstraction over ApplicationDbContext used by Application layer handlers.
/// Implemented by SBS.Infrastructure.Data.ApplicationDbContext.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Booking> Bookings { get; }
    DbSet<BookingService> BookingServices { get; }
    DbSet<CustomerCheckin> CustomerCheckins { get; }
    DbSet<Discount> Discounts { get; }
    DbSet<Payment> Payments { get; }
    DbSet<Pool> Pools { get; }
    DbSet<PoolService> PoolServices { get; }
    DbSet<PoolTicketType> PoolTicketTypes { get; }
    DbSet<SaleTicketDirectly> SaleTicketDirectlys { get; }
    DbSet<ServiceReport> ServiceReports { get; }
    DbSet<Staff> Staffs { get; }
    DbSet<Ticket> Tickets { get; }
    DbSet<TicketType> TicketTypes { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
