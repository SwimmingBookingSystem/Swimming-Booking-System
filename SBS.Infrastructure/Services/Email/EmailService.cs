using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SBS.Application.Common.Interfaces;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace SBS.Infrastructure.Services.Email;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var smtpSection = _configuration.GetSection("Smtp");
        var host = smtpSection["Host"] ?? _configuration["SmtpSettings:Host"];
        var portStr = smtpSection["Port"] ?? _configuration["SmtpSettings:Port"];
        var userName = smtpSection["Username"] ?? smtpSection["UserName"] ?? _configuration["SmtpSettings:UserName"];
        var password = smtpSection["AppPassword"] ?? smtpSection["Password"] ?? _configuration["SmtpSettings:Password"];
        var fromName = smtpSection["FromName"] ?? _configuration["SmtpSettings:FromName"] ?? "Swimming Booking System";

        // Fallback to console logging if SMTP settings are not fully configured
        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
        {
            _logger.LogWarning("SMTP is not fully configured in settings. Fallback: Outputting email to console.");
            Console.WriteLine("=================================================================");
            Console.WriteLine($"[EMAIL TO]: {to}");
            Console.WriteLine($"[SUBJECT]: {subject}");
            Console.WriteLine($"[BODY]: {body}");
            Console.WriteLine("=================================================================");
            return;
        }

        int.TryParse(portStr, out var port);
        if (port == 0) port = 587;

        try
        {
            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(userName, password),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(userName, fromName, System.Text.Encoding.UTF8),
                Subject = subject,
                SubjectEncoding = System.Text.Encoding.UTF8,
                Body = body,
                BodyEncoding = System.Text.Encoding.UTF8,
                HeadersEncoding = System.Text.Encoding.UTF8,
                IsBodyHtml = true
            };
            mailMessage.To.Add(to);

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation($"Email sent successfully to {to}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send email to {to} via SMTP. Fallback: Outputting email to console.");
            Console.WriteLine("=================================================================");
            Console.WriteLine($"[EMAIL TO (SMTP FAILED)]: {to}");
            Console.WriteLine($"[SUBJECT]: {subject}");
            Console.WriteLine($"[BODY]: {body}");
            Console.WriteLine("=================================================================");
        }
    }
}
