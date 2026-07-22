using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBS.Application.Features.Customer_Bookings.Commands.CancelBooking;
using SBS.Application.Features.Customer_Bookings.Commands.CreateBooking;
using SBS.Application.Features.Customer_Bookings.Commands.JoinWaitlist;
using SBS.Application.Features.Customer_Bookings.Commands.ProcessPaymentWebhook;
using SBS.Application.Features.Customer_Bookings.Dtos;
using SBS.Application.Features.Customer_Bookings.Queries.GetAvailableSlots;
using SBS.Application.Features.Customer_Bookings.Queries.GetCustomerBookings;
using SBS.Application.Features.Customer_Bookings.Queries.GetCustomerWaitlists;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SBS.Api.Controllers.Customer;

[ApiController]
[Route("api/customer-bookings")]
[Authorize(Roles = "Customer")]
public class CustomerBookingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomerBookingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("pools/{poolId}/available-slots")]
    public async Task<ActionResult<List<AvailableSlotDto>>> GetAvailableSlots([FromRoute] int poolId, [FromQuery] DateOnly date)
    {
        var result = await _mediator.Send(new GetAvailableSlotsQuery(poolId, date));
        return Ok(result);
    }

    [HttpGet("pools/{poolId}/tickets")]
    public async Task<ActionResult<List<CustomerPoolTicketDto>>> GetPoolTickets([FromRoute] int poolId)
    {
        var result = await _mediator.Send(new SBS.Application.Features.Customer_Bookings.Queries.GetPoolTickets.GetPoolTicketsQuery(poolId));
        return Ok(result);
    }

    [HttpPost("create")]
    public async Task<ActionResult<CreateBookingResponseDto>> CreateBooking([FromBody] CreateBookingCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("{bookingId}/cancel")]
    public async Task<IActionResult> CancelBooking([FromRoute] int bookingId)
    {
        var command = new CancelBookingCommand(bookingId);
        await _mediator.Send(command);
        return Ok(new { message = "Hủy vé thành công" });
    }

    [HttpPost("payos-webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> PayOSWebhook()
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();

        var command = new ProcessPaymentWebhookCommand { WebhookBody = body };
        await _mediator.Send(command);

        // Always return 200 OK if successful
        return Ok(new { message = "Webhook processed successfully" });
    }

    [HttpPost("waitlist/join")]
    public async Task<ActionResult<JoinWaitlistResultDto>> JoinWaitlist([FromBody] JoinWaitlistCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("waitlist/cancel")]
    public async Task<IActionResult> CancelWaitlist([FromBody] SBS.Application.Features.Customer_Bookings.Commands.CancelWaitlist.CancelWaitlistCommand command)
    {
        await _mediator.Send(command);
        return Ok(new { message = "Thành công" });
    }

    [HttpGet("history")]
    public async Task<ActionResult<List<CustomerBookingHistoryDto>>> GetBookingHistory()
    {
        var result = await _mediator.Send(new GetCustomerBookingsQuery());
        return Ok(result);
    }

    [HttpGet("waitlist/my-waitlists")]
    public async Task<ActionResult<List<CustomerWaitlistDto>>> GetMyWaitlists()
    {
        var result = await _mediator.Send(new GetCustomerWaitlistsQuery());
        return Ok(result);
    }
}
