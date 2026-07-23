using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SBS.Application.Common.Interfaces;
using SBS.Application.Common;
using SBS.Application.Features.Customer_Bookings.Events;
using SBS.Application.Features.Customer_Bookings.Interfaces;
using SBS.Application.Features.Customer_Bookings.Policies;
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
    private readonly IBookingCalculationService _bookingCalculationService;
    private readonly ILogger<SlotCapacityFreedEventConsumer> _logger;

    public SlotCapacityFreedEventConsumer(
        IUnitOfWork unitOfWork,
        IPayOSService payOSService,
        SBS.Application.Features.Customer_Bookings.Interfaces.IEmailService emailService,
        IIdentityService identityService,
        IPoolSlotBookingRepository poolSlotBookingRepository,
        IBookingCalculationService bookingCalculationService,
        ILogger<SlotCapacityFreedEventConsumer> logger)
    {
        _unitOfWork = unitOfWork;
        _payOSService = payOSService;
        _emailService = emailService;
        _identityService = identityService;
        _poolSlotBookingRepository = poolSlotBookingRepository;
        _bookingCalculationService = bookingCalculationService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SlotCapacityFreedEvent> context)
    {
        var poolSlotId = context.Message.PoolSlotId;
        _logger.LogInformation("Bắt đầu xử lý hàng chờ cho ca bơi {PoolSlotId}.", poolSlotId);

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

            var utcNow = DateTime.UtcNow;
            var (currentDate, currentTime) = BookingTimePolicy.GetVietnamDateAndTime(utcNow);
            if (BookingTimePolicy.IsBookingClosed(poolSlot.SlotDate, poolSlot.EndTime, currentDate, currentTime))
            {
                var waitingEntries = await _unitOfWork.Repository<WaitlistEntry>().Query()
                    .Where(w => w.PoolSlotId == poolSlotId && w.Status == WaitlistStatus.Waiting)
                    .ToListAsync(context.CancellationToken);

                foreach (var waitingEntry in waitingEntries)
                {
                    waitingEntry.Status = WaitlistStatus.Expired;
                    _unitOfWork.Repository<WaitlistEntry>().Update(waitingEntry);
                }

                await _unitOfWork.SaveChangesAsync(context.CancellationToken);
                await _unitOfWork.CommitTransactionAsync(context.CancellationToken);
                _logger.LogInformation(
                    "Đã đóng hàng chờ ca bơi {PoolSlotId} vì thời gian bơi còn lại không quá 30 phút.",
                    poolSlotId);
                return;
            }

            var bookingCutoffUtc = BookingTimePolicy.GetBookingCutoffUtc(poolSlot.SlotDate, poolSlot.EndTime);

            var pool = await _unitOfWork.Repository<Pool>().Query()
                .FirstOrDefaultAsync(p => p.PoolId == poolSlot.PoolId, context.CancellationToken);
            if (pool == null)
            {
                throw new InvalidOperationException($"Kh\u00f4ng t\u00ecm th\u1ea5y b\u1ec3 b\u01a1i {poolSlot.PoolId} c\u1ee7a ca b\u01a1i {poolSlotId}.");
            }

            var hasActiveOffer = await _unitOfWork.Repository<WaitlistEntry>().Query()
                .AnyAsync(w => w.PoolSlotId == poolSlotId && w.Status == WaitlistStatus.Offered &&
                               w.Deadline.HasValue && w.Deadline > utcNow, context.CancellationToken);
            if (hasActiveOffer)
            {
                _logger.LogInformation("Ca bơi {PoolSlotId} đang có một offer chờ thanh toán; giữ nguyên thứ tự FIFO.", poolSlotId);
                await _unitOfWork.RollbackTransactionAsync(context.CancellationToken);
                return;
            }

            var availableCapacity = await _bookingCalculationService.GetAvailableCapacityAsync(poolSlotId, poolSlot.Capacity, context.CancellationToken);

            if (availableCapacity <= 0)
            {
                _logger.LogInformation("Ca bơi {PoolSlotId} vẫn đầy; chưa thể mời người trong hàng chờ.", poolSlotId);
                await _unitOfWork.RollbackTransactionAsync(context.CancellationToken);
                return;
            }

            while (availableCapacity > 0)
            {
                // Find the next person in waitlist who fits in the available capacity
                var waitlistEntry = await _unitOfWork.Repository<WaitlistEntry>().Query()
                    .Where(w => w.PoolSlotId == poolSlotId && w.Status == WaitlistStatus.Waiting)
                    .OrderBy(w => w.Position)
                    .ThenBy(w => w.CreatedAt)
                    .FirstOrDefaultAsync(context.CancellationToken);

                if (waitlistEntry == null)
                {
                    _logger.LogInformation("Ca bơi {PoolSlotId} còn {Capacity} chỗ nhưng không còn người đang chờ.", poolSlotId, availableCapacity);
                    break; // break the loop, continue to commit transaction
                }

                var ticket = await _unitOfWork.Repository<PoolTicketType>().Query()
                    .Include(t => t.TicketType)
                    .Where(t => t.PoolId == poolSlot.PoolId && t.Status == "Active" &&
                                t.TicketType.Status == "Active" && t.TicketType.Category == "Single")
                    .OrderBy(t => t.PoolTicketTypeId).FirstOrDefaultAsync(context.CancellationToken);
                if (ticket == null) 
                {
                    _logger.LogWarning("Bể bơi {PoolId} không có vé đơn đang hoạt động; không thể tạo offer hàng chờ.", poolSlot.PoolId);
                    break;
                }

                var offerNow = DateTime.UtcNow;
                var paymentDeadline = offerNow.AddMinutes(5) < bookingCutoffUtc
                    ? offerNow.AddMinutes(5)
                    : bookingCutoffUtc;
                var paymentWindowMinutes = Math.Max(
                    1, (int)Math.Ceiling((paymentDeadline - offerNow).TotalMinutes));

                // Update Waitlist status
                waitlistEntry.Status = WaitlistStatus.Offered;
                waitlistEntry.NotifiedAt = offerNow;
                waitlistEntry.Deadline = paymentDeadline;
                
                var ticketPrice = ticket.Price ?? (ticket.TicketType != null ? ticket.TicketType.BasePrice * (1 - ticket.TicketType.DiscountPercent / 100m) : 0m);

                // Create a Booking
                var booking = new Booking
                {
                    UserId = waitlistEntry.UserId,
                    PoolSlotId = poolSlotId,
                    BookingCode = $"WL{DateTime.UtcNow:yyyyMMddHHmmss}{waitlistEntry.UserId.ToString().Substring(0,4).ToUpper()}",
                    BookingDate = poolSlot.SlotDate,
                    Status = BookingStatus.PendingPayment,
                    PaymentDeadline = paymentDeadline,
                    TotalAmount = ticketPrice,
                    BookingType = "Online"
                };

                await _unitOfWork.Repository<Booking>().AddAsync(booking, context.CancellationToken);
                await _unitOfWork.SaveChangesAsync(context.CancellationToken); // save to get BookingId

                var bookingDetail = new BookingDetail
                {
                    BookingId = booking.BookingId,
                    PoolTicketTypeId = ticket.PoolTicketTypeId,
                    Quantity = 1,
                    UnitPrice = ticketPrice,
                    SubTotal = ticketPrice
                };
                await _unitOfWork.Repository<BookingDetail>().AddAsync(bookingDetail, context.CancellationToken);
                
                waitlistEntry.BookingId = booking.BookingId;
                _unitOfWork.Repository<WaitlistEntry>().Update(waitlistEntry);
                await _unitOfWork.SaveChangesAsync(context.CancellationToken);

                _logger.LogInformation("Đã tạo booking {BookingId} cho lượt hàng chờ {WaitlistId}.", booking.BookingId, waitlistEntry.WaitlistEntryId);
                

                // Generate PayOS Link
                var paymentUrl = await _payOSService.CreatePaymentLinkAsync(
                    booking.BookingId, 
                    booking.TotalAmount, 
                    booking.BookingCode, 
                    booking.PaymentDeadline.Value);

                // Send Email
                var userProfile = await _identityService.GetProfileAsync(waitlistEntry.UserId, context.CancellationToken);
                if (userProfile == null || string.IsNullOrWhiteSpace(userProfile.Email))
                {
                    throw new InvalidOperationException($"Người dùng của lượt hàng chờ {waitlistEntry.WaitlistEntryId} không có email hợp lệ.");
                }

                    var body = $"<h3>Xin chào {userProfile.FullName}!</h3>" +
                               $"<p>Hồ bơi <b>{pool.PoolName}</b> ca bơi <b>{poolSlot.StartTime} - {poolSlot.EndTime}</b> vừa có chỗ trống.</p>" +
                               $"<p>Hệ thống đã giữ <b>1 vé đơn</b> cho bạn. Bạn có <b>{paymentWindowMinutes} phút</b> để hoàn tất thanh toán.</p>" +
                               $"<p><a href='{paymentUrl}'>Bấm vào đây để thanh toán ngay</a></p>" +
                               $"<p>Nếu không thanh toán trong {paymentWindowMinutes} phút, vé sẽ được chuyển cho người tiếp theo.</p>";

                    await _emailService.SendEmailWithQrCodeAsync(userProfile.Email, "THÔNG BÁO CÓ VÉ BƠI TỪ DANH SÁCH CHỜ", body, null, null);
                break;
            }

            // Commit the global transaction
            await _unitOfWork.CommitTransactionAsync(context.CancellationToken);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(context.CancellationToken);
            _logger.LogError(ex, "Xử lý hàng chờ thất bại cho ca bơi {PoolSlotId}.", poolSlotId);
            throw; // Rethrow to allow MassTransit to retry
        }
    }
}
