using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SBS.Application.Common.Models;
using SBS.Application.Features.CheckIn.Commands;
using SBS.Application.Features.CheckIn.DTOs;
using SBS.Application.Features.CheckIn.Queries;
using System;
using System.Threading.Tasks;

namespace SBS.Api.Controllers.Staff;

[ApiController]
[Route("api/staff/sale-tickets")]
public class SaleTicketController : ControllerBase
{
    private readonly ISender _sender;

    public SaleTicketController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SaleTicketResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaleTicket([FromBody] SaleTicketRequestDto request)
    {
        try
        {
            var command = new SaleTicketCommand(request);
            var result = await _sender.Send(command);
            return StatusCode(StatusCodes.Status201Created, result);
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

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<SaleTicketListItemDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSaleTicketsHistory(
        [FromQuery] int poolId,
        [FromQuery] DateOnly? date = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var query = new GetSaleTicketsQuery(poolId, date, page, pageSize);
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
