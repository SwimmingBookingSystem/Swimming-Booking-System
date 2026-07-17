using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.Customer_Bookings.Events;
using SBS.Application.Features.Customer_Bookings.Interfaces;
using SBS.Domain.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SBS.Application.Features.Customer_Bookings.Consumers;

public class SlotCapacityFreedEventConsumer : IConsumer<SlotCapacityFreedEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPayOSService _payOSService;
    private readonly SBS.Application.Features.Customer_Bookings.Interfaces.IEmailService _emailService;
    private readonly IIdentityService _identityService;
    private readonly IPoolSlotBookingRepository _poolSlotBookingRepository;
    private readonly ILogger<SlotCapacityFreedEventConsumer> _logger;

    public SlotCapacityFreedEventConsumer(
        IUnitOfWork unitOfWork,
        IPayOSService payOSService,
        SBS.Application.Features.Customer_Bookings.Interfaces.IEmailService emailService,
        IIdentityService identityService,
        IPoolSlotBookingRepository poolSlotBookingRepository,
        ILogger<SlotCapacityFreedEventConsumer> logger)
    {
        _unitOfWork = unitOfWork;
        _payOSService = payOSService;
        _emailService = emailService;
        _identityService = identityService;
        _poolSlotBookingRepository = poolSlotBookingRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SlotCapacityFreedEvent> context)
    {
        var poolSlotId = context.Message.PoolSlotId;
        _logger.LogInformation("Processing Waitlist for PoolSlotId {PoolSlotId}", poolSlotId);

        // Open one global transaction to lock the slot and process waitlists
        await _unitOfWork.BeginTransactionAsync(context.CancellationToken);
        try
        {
            var poolSlot = await _poolSlotBookingRepository.GetPoolSlotWithLockAsync(poolSlotId, context.CancellationToken);

            if (poolSlot == null) 
            {
                await _unitOfWork.RollbackTransactionAsync(context.CancellationToken);
                return;
            }

            // We need Pool.PoolTicketTypes, which wasn't loaded by GetPoolSlotWithLockAsync
            // So we explicit load it
            await _unitOfWork.Repository<PoolSlot>().Query().Include(s => s.Pool).ThenInclude(p => p.PoolTicketTypes)
                .Where(s => s.PoolSlotId == poolSlotId).FirstOrDefaultAsync(context.CancellationToken);

            var currentBookedDetails = await _unitOfWork.Repository<BookingDetail>().Query()
                .Include(bd => bd.PoolTicketType)
                    .ThenInclude(pt => pt.TicketType)
                        .ThenInclude(tt => tt.ComboItems)
                .Where(bd => bd.Booking.PoolSlotId == poolSlotId && 
                             bd.Booking.Status != "Cancelled" && 
                             bd.Booking.Status != "Failed" &&
                             bd.Booking.Status != "Refunded")
                .ToListAsync(context.CancellationToken);

            int bookedCapacity = currentBookedDetails.Sum(bd => 
            {
                var tt = bd.PoolTicketType.TicketType;
                int slotEq = tt.Category == "Combo" ? tt.ComboItems.Sum(c => c.Quantity) : 1;
                return bd.Quantity * slotEq;
            });

            var availableCapacity = poolSlot.Capacity - bookedCapacity;

            if (availableCapacity <= 0)
            {
                _logger.LogInformation("PoolSlotId {PoolSlotId} is still full.", poolSlotId);
                await _unitOfWork.RollbackTransactionAsync(context.CancellationToken);
                return;
            }

            while (availableCapacity > 0)
            {
                // Find the next person in waitlist who fits in the available capacity
                var waitlistEntry = await _unitOfWork.Repository<WaitlistEntry>().Query()
                    .Where(w => w.PoolSlotId == poolSlotId && w.Status == "Waiting" && w.Quantity <= availableCapacity)
                    .OrderBy(w => w.Position)
                    .ThenBy(w => w.CreatedAt)
                    .FirstOrDefaultAsync(context.CancellationToken);

                if (waitlistEntry == null)
                {
                    _logger.LogInformation("No suitable waitlist entry found for PoolSlotId {PoolSlotId} with capacity {Cap}.", poolSlotId, availableCapacity);
                    break; // break the loop, continue to commit transaction
                }

                var ticket = poolSlot.Pool?.PoolTicketTypes?.FirstOrDefault(t => t.Status == "Active" && t.TicketType?.Category == "Single");
                if (ticket == null) 
                {
                    _logger.LogWarning("No active ticket types found for pool {PoolId}.", poolSlot.PoolId);
                    break;
                }

                // Update Waitlist status
                waitlistEntry.Status = "Offered";
                waitlistEntry.NotifiedAt = DateTime.UtcNow;
                
                var ticketPrice = ticket.Price ?? (ticket.TicketType != null ? ticket.TicketType.BasePrice * (1 - ticket.TicketType.DiscountPercent / 100m) : 0m);

                // Create a Booking
                var booking = new Booking
                {
                    UserId = waitlistEntry.UserId,
                    PoolSlotId = poolSlotId,
                    BookingCode = $"WL{DateTime.UtcNow:yyyyMMddHHmmss}{waitlistEntry.UserId.ToString().Substring(0,4).ToUpper()}",
                    BookingDate = poolSlot.SlotDate,
                    Status = "PendingPayment", // 5 minutes to pay
                    PaymentDeadline = DateTime.UtcNow.AddMinutes(5),
                    TotalAmount = ticketPrice * waitlistEntry.Quantity,
                    BookingType = "Online"
                };

                await _unitOfWork.Repository<Booking>().AddAsync(booking, context.CancellationToken);
                await _unitOfWork.SaveChangesAsync(context.CancellationToken); // save to get BookingId

                var bookingDetail = new BookingDetail
                {
                    BookingId = booking.BookingId,
                    PoolTicketTypeId = ticket.PoolTicketTypeId,
                    Quantity = waitlistEntry.Quantity,
                    UnitPrice = ticketPrice,
                    SubTotal = ticketPrice * waitlistEntry.Quantity
                };
                await _unitOfWork.Repository<BookingDetail>().AddAsync(bookingDetail, context.CancellationToken);
                
                _unitOfWork.Repository<WaitlistEntry>().Update(waitlistEntry);
                await _unitOfWork.SaveChangesAsync(context.CancellationToken);

                _logger.LogInformation("Created Booking {BookingId} for WaitlistEntry {WaitlistId}", booking.BookingId, waitlistEntry.WaitlistEntryId);
                
                // Deduct assigned capacity so loop can continue
                availableCapacity -= waitlistEntry.Quantity;

                // Generate PayOS Link
                var paymentUrl = await _payOSService.CreatePaymentLinkAsync(
                    booking.BookingId, 
                    booking.TotalAmount, 
                    booking.BookingCode, 
                    booking.PaymentDeadline.Value);

                // Send Email
                var userProfile = await _identityService.GetProfileAsync(waitlistEntry.UserId, context.CancellationToken);
                if (userProfile != null)
                {
                    var body = $"<h3>Xin chào {userProfile.FullName}!</h3>" +
                               $"<p>Hồ bơi <b>{poolSlot.Pool?.PoolName}</b> ca bơi <b>{poolSlot.StartTime} - {poolSlot.EndTime}</b> vừa có chỗ trống.</p>" +
                               $"<p>Hệ thống đã giữ {waitlistEntry.Quantity} vé cho bạn. Bạn có <b>5 phút</b> để hoàn tất thanh toán.</p>" +
                               $"<p><a href='{paymentUrl}'>Bấm vào đây để thanh toán ngay</a></p>" +
                               $"<p>Nếu không thanh toán trong 5 phút, vé sẽ được chuyển cho người tiếp theo.</p>";

                    await _emailService.SendEmailWithQrCodeAsync(userProfile.Email, "THÔNG BÁO CÓ VÉ BƠI TỪ DANH SÁCH CHỜ", body, null, null);
                }
            }

            // Commit the global transaction
            await _unitOfWork.CommitTransactionAsync(context.CancellationToken);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(context.CancellationToken);
            _logger.LogError(ex, "Error processing waitlist entries for PoolSlotId {PoolSlotId}", poolSlotId);
            throw; // Rethrow to allow MassTransit to retry
        }
    }
}
