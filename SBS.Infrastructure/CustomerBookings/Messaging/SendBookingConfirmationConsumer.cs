using MassTransit;
using SBS.Application.Features.Customer_Bookings.Events;
using SBS.Application.Features.Customer_Bookings.Interfaces;
using System.Threading.Tasks;

namespace SBS.Infrastructure.CustomerBookings.Messaging;

public class SendBookingConfirmationConsumer : IConsumer<BookingConfirmedEvent>
{
    private readonly IQRCodeService _qrCodeService;
    private readonly IEmailService _emailService;

    public SendBookingConfirmationConsumer(IQRCodeService qrCodeService, IEmailService emailService)
    {
        _qrCodeService = qrCodeService;
        _emailService = emailService;
    }

    public async Task Consume(ConsumeContext<BookingConfirmedEvent> context)
    {
        var message = context.Message;

        // Generate QR code image
        var qrCodeImage = _qrCodeService.GenerateQrCode(message.QrCodeData);

        // Send Email
        var subject = $"[SBS] Xác nhận đặt vé hồ bơi - Mã: {message.BookingCode}";
        var body = $@"
            <h2>Cảm ơn bạn đã đặt vé tại Swimming Booking System (SBS)</h2>
            <p>Mã đặt vé của bạn là: <strong>{message.BookingCode}</strong></p>
            <p>Vui lòng mang theo mã QR được đính kèm trong email này để check-in tại quầy vé.</p>
            <p>Trân trọng,<br>Ban quản lý SBS.</p>
        ";

        await _emailService.SendEmailWithQrCodeAsync(message.UserEmail, subject, body, qrCodeImage, $"{message.BookingCode}.png");
    }
}
