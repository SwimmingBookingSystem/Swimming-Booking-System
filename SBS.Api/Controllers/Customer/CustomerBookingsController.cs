using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBS.Application.Features.Customer_Bookings.Commands.CreateBooking;
using SBS.Application.Features.Customer_Bookings.Commands.ProcessPaymentWebhook;
using SBS.Application.Features.Customer_Bookings.Dtos;
using SBS.Application.Features.Customer_Bookings.Queries.GetAvailableSlots;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SBS.Api.Controllers.Customer;

[ApiController]
[Route("api/customer-bookings")]
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

    [Authorize]
    [HttpPost("create")]
    public async Task<ActionResult<CreateBookingResponseDto>> CreateBooking([FromBody] CreateBookingCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("payos-webhook")]
    public async Task<IActionResult> PayOSWebhook()
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();

        var command = new ProcessPaymentWebhookCommand { WebhookBody = body };
        await _mediator.Send(command);

        // Always return 200 OK if successful
        return Ok(new { message = "Webhook processed successfully" });
    }
}
