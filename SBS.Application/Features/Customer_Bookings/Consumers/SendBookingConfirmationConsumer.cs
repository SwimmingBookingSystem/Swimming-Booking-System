using MassTransit;
using Microsoft.EntityFrameworkCore;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.Customer_Bookings.Events;
using SBS.Application.Features.Customer_Bookings.Interfaces;
using SBS.Domain.Entities;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBS.Application.Features.Customer_Bookings.Consumers;

public class SendBookingConfirmationConsumer : IConsumer<BookingConfirmedEvent>
{
    private readonly IQRCodeService _qrCodeService;
    private readonly SBS.Application.Features.Customer_Bookings.Interfaces.IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;
    private readonly ITemplateService _templateService;

    public SendBookingConfirmationConsumer(
        IQRCodeService qrCodeService, 
        SBS.Application.Features.Customer_Bookings.Interfaces.IEmailService emailService,
        IUnitOfWork unitOfWork,
        IIdentityService identityService,
        ITemplateService templateService)
    {
        _qrCodeService = qrCodeService;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _identityService = identityService;
        _templateService = templateService;
    }

    public async Task Consume(ConsumeContext<BookingConfirmedEvent> context)
    {
        var message = context.Message;

        // Query Booking details
        var booking = await _unitOfWork.Repository<Booking>().Query()
            .Include(b => b.PoolSlot)
                .ThenInclude(ps => ps.Pool)
            .Include(b => b.BookingDetails)
                .ThenInclude(bd => bd.PoolTicketType)
                    .ThenInclude(ptt => ptt.TicketType)
            .FirstOrDefaultAsync(b => b.BookingId == message.BookingId);

        if (booking == null) return;

        // Fetch user profile for name
        var userProfile = await _identityService.GetProfileAsync(booking.UserId, context.CancellationToken);
        var customerName = userProfile != null ? userProfile.FullName : "Quý khách";

        // Generate QR code image
        var qrCodeImage = _qrCodeService.GenerateQrCode(message.QrCodeData);

        // Ticket details string
        var ticketDetailsBuilder = new StringBuilder();
        foreach (var detail in booking.BookingDetails)
        {
            var ticketName = detail.PoolTicketType?.TicketType?.TicketName ?? "Vé";
            ticketDetailsBuilder.Append($"{detail.Quantity}x {ticketName} ({(int)detail.UnitPrice:N0}đ)<br/>");
        }

        // Read Template
        var templateContent = await _templateService.GetBookingInvoiceTemplateAsync();
        string body;
        
        if (!string.IsNullOrEmpty(templateContent))
        {
            body = templateContent.Replace("{{CustomerName}}", customerName)
                       .Replace("{{BookingCode}}", booking.BookingCode)
                       .Replace("{{PoolName}}", booking.PoolSlot?.Pool?.PoolName ?? "N/A")
                       .Replace("{{BookingDate}}", booking.BookingDate.ToString("dd/MM/yyyy"))
                       .Replace("{{SlotTime}}", $"{booking.PoolSlot?.StartTime:hh\\:mm} - {booking.PoolSlot?.EndTime:hh\\:mm}")
                       .Replace("{{TicketDetails}}", ticketDetailsBuilder.ToString())
                       .Replace("{{TotalAmount}}", ((int)booking.TotalAmount).ToString("N0"));
        }
        else
        {
            // Fallback if template missing
            body = $@"
                <h2>Cảm ơn bạn đã đặt vé tại Swimming Booking System (SBS)</h2>
                <p>Mã đặt vé của bạn là: <strong>{message.BookingCode}</strong></p>
                <p>Vui lòng mang theo mã QR được đính kèm trong email này để check-in tại quầy vé.</p>
            ";
        }

        // Send Email
        var subject = $"[SBS] Hóa đơn & Vé điện tử - Mã: {message.BookingCode}";
        await _emailService.SendEmailWithQrCodeAsync(message.UserEmail, subject, body, qrCodeImage, $"{message.BookingCode}.png");
    }
}
