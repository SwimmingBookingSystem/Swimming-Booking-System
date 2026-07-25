using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SBS.Application.Common;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.Customer_Bookings.Events;
using SBS.Application.Features.Customer_Bookings.Interfaces;
using SBS.Application.Features.Customer_Bookings.Policies;
using SBS.Domain.Entities;
using System;
using System.Collections.Generic;
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
        var offers = new List<WaitlistOfferNotification>();

        await _unitOfWork.BeginTransactionAsync(context.CancellationToken);
        try
        {
            var poolSlot = await _poolSlotBookingRepository.GetPoolSlotWithLockAsync(poolSlotId, context.CancellationToken);
            if (poolSlot is null)
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
                return;
            }

            var availableCapacity = await _bookingCalculationService.GetAvailableCapacityAsync(
                poolSlotId,
                poolSlot.Capacity,
                context.CancellationToken);
            if (availableCapacity <= 0)
            {
                await _unitOfWork.RollbackTransactionAsync(context.CancellationToken);
                return;
            }

            var ticket = await _unitOfWork.Repository<PoolTicketType>().Query()
                .Include(t => t.TicketType)
                .Where(t => t.PoolId == poolSlot.PoolId &&
                            t.Status == "Active" &&
                            t.TicketType.Status == "Active" &&
                            t.TicketType.Category == "Single")
                .OrderBy(t => t.PoolTicketTypeId)
                .FirstOrDefaultAsync(context.CancellationToken);
            if (ticket is null)
            {
                _logger.LogWarning("Pool {PoolId} has no active single ticket; waitlist offers cannot be created.", poolSlot.PoolId);
                await _unitOfWork.RollbackTransactionAsync(context.CancellationToken);
                return;
            }

            var waitingEntriesToOffer = await _unitOfWork.Repository<WaitlistEntry>().Query()
                .Where(w => w.PoolSlotId == poolSlotId && w.Status == WaitlistStatus.Waiting)
                .OrderBy(w => w.Position)
                .ThenBy(w => w.CreatedAt)
                .Take(availableCapacity)
                .ToListAsync(context.CancellationToken);
            if (waitingEntriesToOffer.Count == 0)
            {
                await _unitOfWork.RollbackTransactionAsync(context.CancellationToken);
                return;
            }

            var poolName = await _unitOfWork.Repository<Pool>().Query()
                .Where(p => p.PoolId == poolSlot.PoolId)
                .Select(p => p.PoolName)
                .FirstOrDefaultAsync(context.CancellationToken)
                ?? "bể bơi";
            var bookingCutoffUtc = BookingTimePolicy.GetBookingCutoffUtc(poolSlot.SlotDate, poolSlot.EndTime);
            var ticketPrice = ticket.Price ?? ticket.TicketType.BasePrice * (1 - ticket.TicketType.DiscountPercent / 100m);

            foreach (var waitlistEntry in waitingEntriesToOffer)
            {
                var offerNow = DateTime.UtcNow;
                var paymentDeadline = offerNow.AddMinutes(5) < bookingCutoffUtc
                    ? offerNow.AddMinutes(5)
                    : bookingCutoffUtc;
                if (paymentDeadline <= offerNow)
                {
                    waitlistEntry.Status = WaitlistStatus.Expired;
                    _unitOfWork.Repository<WaitlistEntry>().Update(waitlistEntry);
                    continue;
                }

                waitlistEntry.Status = WaitlistStatus.Offered;
                waitlistEntry.NotifiedAt = offerNow;
                waitlistEntry.Deadline = paymentDeadline;

                var booking = new Booking
                {
                    UserId = waitlistEntry.UserId,
                    PoolSlotId = poolSlotId,
                    BookingCode = $"WL{offerNow:yyyyMMddHHmmss}{waitlistEntry.UserId.ToString()[..4].ToUpperInvariant()}",
                    BookingDate = poolSlot.SlotDate,
                    Status = BookingStatus.PendingPayment,
                    PaymentDeadline = paymentDeadline,
                    TotalAmount = ticketPrice,
                    BookingType = "Online"
                };

                await _unitOfWork.Repository<Booking>().AddAsync(booking, context.CancellationToken);
                await _unitOfWork.SaveChangesAsync(context.CancellationToken);

                await _unitOfWork.Repository<BookingDetail>().AddAsync(new BookingDetail
                {
                    BookingId = booking.BookingId,
                    PoolTicketTypeId = ticket.PoolTicketTypeId,
                    Quantity = 1,
                    UnitPrice = ticketPrice,
                    SubTotal = ticketPrice
                }, context.CancellationToken);

                waitlistEntry.BookingId = booking.BookingId;
                _unitOfWork.Repository<WaitlistEntry>().Update(waitlistEntry);
                offers.Add(new WaitlistOfferNotification(
                    waitlistEntry.UserId,
                    waitlistEntry.WaitlistEntryId,
                    booking.BookingId,
                    booking.BookingCode,
                    booking.TotalAmount,
                    paymentDeadline,
                    poolName,
                    poolSlot.StartTime,
                    poolSlot.EndTime));
            }

            await _unitOfWork.SaveChangesAsync(context.CancellationToken);
            await _unitOfWork.CommitTransactionAsync(context.CancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(context.CancellationToken);
            throw;
        }

        foreach (var offer in offers)
        {
            try
            {
                var paymentUrl = await _payOSService.CreatePaymentLinkAsync(
                    offer.BookingId,
                    offer.TotalAmount,
                    offer.BookingCode,
                    offer.PaymentDeadline);
                var userProfile = await _identityService.GetProfileAsync(offer.UserId, context.CancellationToken);
                if (userProfile is null || string.IsNullOrWhiteSpace(userProfile.Email))
                {
                    throw new InvalidOperationException($"Waitlist user {offer.WaitlistEntryId} has no valid email.");
                }

                var paymentWindowMinutes = Math.Max(1, (int)Math.Ceiling((offer.PaymentDeadline - DateTime.UtcNow).TotalMinutes));
                var body = $"<h3>Xin chào {userProfile.FullName}!</h3>" +
                           $"<p>Hồ bơi <b>{offer.PoolName}</b>, ca <b>{offer.StartTime:hh\\:mm} - {offer.EndTime:hh\\:mm}</b> vừa có chỗ trống.</p>" +
                           $"<p>Hệ thống đã giữ <b>1 vé đơn</b> cho bạn. Bạn có <b>{paymentWindowMinutes} phút</b> để thanh toán.</p>" +
                           $"<p><a href='{paymentUrl}'>Thanh toán ngay</a></p>";

                await _emailService.SendEmailWithQrCodeAsync(
                    userProfile.Email,
                    "THÔNG BÁO CÓ VÉ BƠI TỪ DANH SÁCH CHỜ",
                    body,
                    null,
                    null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send payment offer for waitlist entry {WaitlistEntryId}.", offer.WaitlistEntryId);
            }
        }
    }

    private sealed record WaitlistOfferNotification(
        Guid UserId,
        int WaitlistEntryId,
        int BookingId,
        string BookingCode,
        decimal TotalAmount,
        DateTime PaymentDeadline,
        string PoolName,
        TimeSpan StartTime,
        TimeSpan EndTime);
}