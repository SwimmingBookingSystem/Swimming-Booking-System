using System.Threading.Tasks;

namespace SBS.Application.Features.Customer_Bookings.Interfaces;

public interface IEmailService
{
    Task SendEmailWithQrCodeAsync(string toEmail, string subject, string body, byte[] qrCodeImage, string qrCodeFileName);
}
