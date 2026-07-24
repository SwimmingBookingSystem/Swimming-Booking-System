using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SBS.Application.Common;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.Customer_Bookings.Events;
using SBS.Application.Features.Customer_Bookings.Exceptions;
using SBS.Application.Features.Customer_Bookings.Interfaces;
using SBS.Domain.Entities;

namespace SBS.Application.Features.Customer_Bookings.Commands.ConfirmPayment;

public sealed class ConfirmPaymentCommandHandler : IRequestHandler<ConfirmPaymentCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IIdentityService _identityService;
    private readonly IPayOSService _payOSService;
    private readonly IPublishEndpoint _publishEndpoint;

    public ConfirmPaymentCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IIdentityService identityService,
        IPayOSService payOSService,
        IPublishEndpoint publishEndpoint)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _identityService = identityService;
        _payOSService = payOSService;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<bool> Handle(ConfirmPaymentCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var currentUserId))
        {
            throw new UnauthorizedAccessException("Phiên đăng nhập không hợp lệ hoặc đã hết hạn.");
        }

        var booking = await _unitOfWork.Repository<Booking>().Query()
            .FirstOrDefaultAsync(b => b.BookingId == request.BookingId, cancellationToken)
            ?? throw new BookingNotFoundException(request.BookingId);

        if (booking.UserId != currentUserId)
        {
            throw new UnauthorizedAccessException("Bạn không có quyền xác nhận thanh toán cho đơn đặt vé này.");
        }

        if (booking.Status == "Paid")
        {
            return true;
        }

        // PayOS is the payment source of truth. This also recovers a booking that
        // the expiration worker canceled while a local webhook was unreachable.
        var paymentInformation = await _payOSService.GetPaymentInformationAsync(request.BookingId);
        var expectedAmount = decimal.Truncate(booking.TotalAmount);

        if (paymentInformation.OrderCode != request.BookingId
            || !string.Equals(paymentInformation.Status, "PAID", StringComparison.OrdinalIgnoreCase)
            || paymentInformation.Amount != expectedAmount
            || paymentInformation.AmountPaid < expectedAmount)
        {
            throw new InvalidOperationException("PayOS chưa xác nhận giao dịch đã thanh toán đủ.");
        }

        var userProfile = await _identityService.GetProfileAsync(booking.UserId, cancellationToken);
        if (userProfile == null || string.IsNullOrWhiteSpace(userProfile.Email))
        {
            throw new InvalidOperationException("Tài khoản chưa có email hợp lệ để nhận vé điện tử.");
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        var purchasedFromWaitlist = false;
        try
        {
            // Reload inside the transaction so webhook and return-url reconciliation
            // cannot create two successful payments for the same booking.
            booking = await _unitOfWork.Repository<Booking>().Query()
                .FirstAsync(b => b.BookingId == request.BookingId, cancellationToken);

            var waitlistEntry = await _unitOfWork.Repository<WaitlistEntry>().Query()
                .FirstOrDefaultAsync(w => w.BookingId == booking.BookingId, cancellationToken);
            if (waitlistEntry?.Status == WaitlistStatus.Offered &&
                waitlistEntry.Deadline.HasValue && waitlistEntry.Deadline <= DateTime.UtcNow)
            {
                throw new InvalidOperationException(
                    "Quyền ưu tiên từ hàng chờ đã hết hạn. Vé đã được chuyển cho người tiếp theo.");
            }

            var existingPayment = await _unitOfWork.Repository<Payment>().Query()
                .FirstOrDefaultAsync(
                    p => p.BookingId == booking.BookingId && p.Status == "Success",
                    cancellationToken);

            if (booking.Status == "Paid" || existingPayment != null)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return true;
            }

            booking.Status = BookingStatus.Paid;
            booking.UpdatedAt = DateTime.UtcNow;
            booking.QrCodeData ??= $"{booking.BookingCode}-{Guid.NewGuid()}";

            await _unitOfWork.Repository<Payment>().AddAsync(new Payment
            {
                BookingId = booking.BookingId,
                PaymentMethod = "PayOS",
                TransactionId = string.IsNullOrWhiteSpace(paymentInformation.TransactionReference)
                    ? $"payos-order-{paymentInformation.OrderCode}"
                    : paymentInformation.TransactionReference,
                Amount = booking.TotalAmount,
                PaymentDate = DateTime.UtcNow,
                Status = "Success"
            }, cancellationToken);

            if (waitlistEntry?.Status == WaitlistStatus.Offered)
            {
                waitlistEntry.Status = WaitlistStatus.Purchased;
                _unitOfWork.Repository<WaitlistEntry>().Update(waitlistEntry);
                purchasedFromWaitlist = true;
            }
            _unitOfWork.Repository<Booking>().Update(booking);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        await _publishEndpoint.Publish(new BookingConfirmedEvent
        {
            BookingId = booking.BookingId,
            BookingCode = booking.BookingCode,
            UserEmail = userProfile.Email,
            QrCodeData = booking.QrCodeData!
        }, cancellationToken);


        if (purchasedFromWaitlist)
        {
            await _publishEndpoint.Publish(new SlotCapacityFreedEvent
            {
                PoolSlotId = booking.PoolSlotId
            }, cancellationToken);
        }
        return true;
    }
}
