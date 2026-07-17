using System.Threading.Tasks;

namespace SBS.Application.Features.Customer_Bookings.Interfaces;

public interface ITemplateService
{
    Task<string> GetBookingInvoiceTemplateAsync();
}
