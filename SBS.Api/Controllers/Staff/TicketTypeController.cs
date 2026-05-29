using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SBS.Application.Features.CheckIn.DTOs;
using SBS.Application.Features.CheckIn.Queries;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SBS.Api.Controllers.Staff;

[ApiController]
[Route("api/staff/ticket-types")]
public class TicketTypeController : ControllerBase
{
    private readonly ISender _sender;

    public TicketTypeController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IReadOnlyList<TicketTypeDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTicketTypes([FromQuery] int poolId)
    {
        try
        {
            var query = new GetTicketTypesQuery(poolId);
            var result = await _sender.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                statusCode = 400,
                error = "Bad Request",
                message = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }
}
