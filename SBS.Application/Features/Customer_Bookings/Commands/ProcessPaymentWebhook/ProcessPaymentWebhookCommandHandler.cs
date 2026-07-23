using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.Customer_Bookings.Events;
using SBS.Application.Features.Customer_Bookings.Exceptions;
using SBS.Application.Features.Customer_Bookings.Interfaces;
using SBS.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Customer_Bookings.Commands.ProcessPaymentWebhook;

public class ProcessPaymentWebhookCommandHandler : IRequestHandler<ProcessPaymentWebhookCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPayOSService _payOSService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IIdentityService _identityService;

    public ProcessPaymentWebhookCommandHandler(
        IUnitOfWork unitOfWork,
        IPayOSService payOSService,
        IPublishEndpoint publishEndpoint,
        IIdentityService identityService)
    {
        _unitOfWork = unitOfWork;
        _payOSService = payOSService;
        _publishEndpoint = publishEndpoint;
        _identityService = identityService;
    }

    public async Task<bool> Handle(ProcessPaymentWebhookCommand request, CancellationToken cancellationToken)
    {
        // 1. Verify Webhook Signature using PayOSService
        var transactionId = await _payOSService.VerifyPaymentWebhookDataAsync(request.WebhookBody);

        if (!int.TryParse(transactionId, out var bookingId))
        {
            throw new InvalidPaymentWebhookException("Mã đơn hàng (orderCode) trả về từ Webhook không hợp lệ.");
        }

        // 2. Open Transaction for Idempotency and Updates
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Idempotency check: avoid double processing
            var existingPayment = await _unitOfWork.Repository<Payment>().Query()
                .FirstOrDefaultAsync(p => p.TransactionId == transactionId, cancellationToken);
                
            if (existingPayment != null && existingPayment.Status == "Success")
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return true;
            }

            var booking = await _unitOfWork.Repository<Booking>().Query()
                .FirstOrDefaultAsync(b => b.BookingId == bookingId, cancellationToken);

            if (booking == null)
            {
                // Return true to acknowledge the webhook (especially for PayOS "Test Webhook" which sends orderCode=123)
                // This prevents PayOS from continuously retrying the webhook
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return true;
            }

            if (booking.Status == "Paid" || booking.Status == "Cancelled")
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return true;
            }

            // 3. Update Booking and create Payment record
            booking.Status = "Paid";
            booking.UpdatedAt = DateTime.UtcNow;
            
            // Generate simple QrCodeData token for Check-in module
            booking.QrCodeData = $"{booking.BookingCode}-{Guid.NewGuid()}";

            var payment = new Payment
            {
                BookingId = booking.BookingId,
                PaymentMethod = "PayOS",
                TransactionId = transactionId,
                Amount = booking.TotalAmount,
                PaymentDate = DateTime.UtcNow,
                Status = "Success"
            };

            await _unitOfWork.Repository<Payment>().AddAsync(payment, cancellationToken);
            _unitOfWork.Repository<Booking>().Update(booking);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // 4. Publish Event for async processing (QR code generation & Email)
            var userProfile = await _identityService.GetProfileAsync(booking.UserId, cancellationToken);
            if (userProfile != null)
            {
                await _publishEndpoint.Publish(new BookingConfirmedEvent
                {
                    BookingId = booking.BookingId,
                    BookingCode = booking.BookingCode,
                    UserEmail = userProfile.Email,
                    QrCodeData = booking.QrCodeData
                }, cancellationToken);
            }

            return true;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
