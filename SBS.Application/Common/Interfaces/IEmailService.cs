using System.Threading.Tasks;

namespace SBS.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
}
