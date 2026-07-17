using Microsoft.Extensions.Configuration;
using SBS.Application.Features.Customer_Bookings.Interfaces;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace SBS.Infrastructure.CustomerBookings.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailWithQrCodeAsync(string toEmail, string subject, string body, byte[]? qrCodeImage, string? qrCodeFileName)
    {
        var emailSettings = _configuration.GetSection("Smtp");
        var host = emailSettings["Host"];
        var port = int.Parse(emailSettings["Port"] ?? "587");
        var username = emailSettings["Username"];
        var password = emailSettings["AppPassword"];

        using var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(username, password),
            EnableSsl = true
        };

        using var mailMessage = new MailMessage
        {
            From = new MailAddress(username!),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        mailMessage.To.Add(toEmail);

        if (qrCodeImage != null && qrCodeImage.Length > 0)
        {
            using var ms = new MemoryStream(qrCodeImage);
            var attachment = new Attachment(ms, qrCodeFileName, "image/png");
            mailMessage.Attachments.Add(attachment);
            await client.SendMailAsync(mailMessage);
        }
        else
        {
            await client.SendMailAsync(mailMessage);
        }
    }
}
