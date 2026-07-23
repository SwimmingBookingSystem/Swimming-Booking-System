using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SBS.Application.Features.Customer_Bookings.Events;
using SBS.Application.Features.Customer_Bookings.Policies;
using SBS.Infrastructure.Data;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Infrastructure.CustomerBookings.BackgroundServices;

public class PendingBookingExpirationWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PendingBookingExpirationWorker> _logger;

    public PendingBookingExpirationWorker(IServiceProvider serviceProvider, ILogger<PendingBookingExpirationWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessExpiredBookingsAsync(stoppingToken);
                await ProcessExpiredWaitlistsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing PendingBookingExpirationWorker.");
            }

            // Run every 1 minute
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task ProcessExpiredBookingsAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        var now = DateTime.UtcNow;

        // Find all bookings that are PendingPayment and deadline passed
        var expiredBookings = await context.Bookings
            .Include(b => b.BookingDetails)
            .Where(b => b.Status == "PendingPayment" && b.PaymentDeadline < now)
            .ToListAsync(stoppingToken);

        if (!expiredBookings.Any()) return;

        foreach (var booking in expiredBookings)
        {
            booking.Status = "Cancelled";
            booking.UpdatedAt = now;

            // Nếu booking này thuộc về một Waitlist, đánh dấu Waitlist là Expired
            var waitlistEntry = await context.WaitlistEntries
                .FirstOrDefaultAsync(w => w.UserId == booking.UserId && w.PoolSlotId == booking.PoolSlotId && w.Status == "Offered", stoppingToken);
            if (waitlistEntry != null)
            {
                waitlistEntry.Status = "Expired";
            }

            _logger.LogInformation("Cancelled booking {BookingId} due to payment timeout.", booking.BookingId);
        }

        await context.SaveChangesAsync(stoppingToken);

        // Phát sự kiện Waitlist cho các slot đã giải phóng (Distinct để tránh spam)
        var uniquePoolSlotIds = expiredBookings.Select(b => b.PoolSlotId).Distinct();
        foreach (var slotId in uniquePoolSlotIds)
        {
            await publishEndpoint.Publish(new SlotCapacityFreedEvent
            {
                PoolSlotId = slotId
            }, stoppingToken);
        }
    }

    private async Task ProcessExpiredWaitlistsAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var (currentDate, currentTime) = BookingTimePolicy.GetVietnamDateAndTime(DateTime.UtcNow);

        // Fetch potential expired waitlists (Waiting status and SlotDate <= today)
        var potentialExpiredWaitlists = await context.WaitlistEntries
            .Include(w => w.PoolSlot)
            .Where(w => w.Status == "Waiting" && w.PoolSlot.SlotDate <= currentDate)
            .ToListAsync(stoppingToken);

        // Filter in-memory to avoid EF Core TimeSpan arithmetic translation issues
        var actuallyExpired = potentialExpiredWaitlists.Where(w =>
            BookingTimePolicy.IsBookingClosed(
                w.PoolSlot.SlotDate, w.PoolSlot.EndTime, currentDate, currentTime)
        ).ToList();

        if (!actuallyExpired.Any()) return;

        foreach (var waitlist in actuallyExpired)
        {
            waitlist.Status = "Expired";
            _logger.LogInformation("Expired waitlist {WaitlistEntryId} because the slot {PoolSlotId} is less than 30 mins from ending.", waitlist.WaitlistEntryId, waitlist.PoolSlotId);
        }

        await context.SaveChangesAsync(stoppingToken);
    }
}
