using SBS.Application.Features.Customer_Bookings.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SBS.Infrastructure.CustomerBookings.Services;

public class TemplateService : ITemplateService
{
    public async Task<string> GetBookingInvoiceTemplateAsync()
    {
        var templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CustomerBookings", "Templates", "BookingInvoiceTemplate.html");
        if (File.Exists(templatePath))
        {
            return await File.ReadAllTextAsync(templatePath);
        }
        return string.Empty;
    }
}
