using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SBS.Application.Features.Customer_Bookings.Commands.CancelBooking;
using System.Threading.Tasks;

namespace SBS.WebApp.Pages.Customer.Bookings;

public class PaymentResultModel : PageModel
{
    private readonly IMediator _mediator;

    public PaymentResultModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public string Code { get; set; }
    public string Status { get; set; }
    public string OrderCode { get; set; }
    public bool IsSuccess { get; set; }
    public bool IsCanceled { get; set; }

    public async Task OnGetAsync(string code, string id, bool cancel, string status, string orderCode)
    {
        Code = code;
        Status = status;
        OrderCode = orderCode;
        IsCanceled = cancel;

        // "00" indicates success in PayOS
        IsSuccess = (code == "00" && !cancel && status == "PAID");

        if (cancel && int.TryParse(orderCode, out int bookingId))
        {
            await _mediator.Send(new CancelBookingCommand(bookingId));
        }
    }
}
