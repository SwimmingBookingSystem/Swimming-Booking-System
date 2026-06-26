using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBS.Application.Features.Manager.PoolTickets.Commands.ClosePoolTicket;
using SBS.Application.Features.Manager.PoolTickets.Commands.OpenPoolTicket;
using SBS.Application.Features.Manager.PoolTickets.Commands.UpdatePoolTicketPrice;
using SBS.Application.Features.Manager.PoolTickets.Queries.GetTicketsByPool;
using System.Linq;
using System.Threading.Tasks;

namespace SBS.Api.Controllers.Manager;

[ApiController]
[Route("api/manager/pools")]
[Authorize(Roles = "Manager")]
public class ManagerPoolTicketController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly IValidator<UpdatePoolTicketPriceCommand> _priceValidator;

    public ManagerPoolTicketController(
        ISender mediator,
        IValidator<UpdatePoolTicketPriceCommand> priceValidator)
    {
        _mediator       = mediator;
        _priceValidator = priceValidator;
    }

    // GET /api/manager/pools/{poolId}/ticket-types
    [HttpGet("{poolId:int}/ticket-types")]
    public async Task<IActionResult> GetTickets(int poolId)
        => Ok(await _mediator.Send(new GetTicketsByPoolQuery(poolId)));

    // PATCH /api/manager/pools/{poolId}/ticket-types/{ticketTypeId}/price
    [HttpPatch("{poolId:int}/ticket-types/{ticketTypeId:int}/price")]
    public async Task<IActionResult> UpdatePrice(
        int poolId, int ticketTypeId,
        [FromBody] UpdatePoolTicketPriceRequest request)
    {
        var command    = new UpdatePoolTicketPriceCommand(poolId, ticketTypeId, request.Price);
        var validation = await _priceValidator.ValidateAsync(command);
        if (!validation.IsValid)
            return BadRequest(new { errors = validation.Errors.Select(e => e.ErrorMessage) });

        return Ok(await _mediator.Send(command));
    }

    // PATCH /api/manager/pools/{poolId}/ticket-types/{ticketTypeId}/close
    [HttpPatch("{poolId:int}/ticket-types/{ticketTypeId:int}/close")]
    public async Task<IActionResult> CloseTicket(int poolId, int ticketTypeId)
        => Ok(await _mediator.Send(new ClosePoolTicketCommand(poolId, ticketTypeId)));

    // PATCH /api/manager/pools/{poolId}/ticket-types/{ticketTypeId}/open
    [HttpPatch("{poolId:int}/ticket-types/{ticketTypeId:int}/open")]
    public async Task<IActionResult> OpenTicket(int poolId, int ticketTypeId)
        => Ok(await _mediator.Send(new OpenPoolTicketCommand(poolId, ticketTypeId)));
}

// ── Request Model 
public class UpdatePoolTicketPriceRequest
{
    public decimal Price { get; set; }
}
