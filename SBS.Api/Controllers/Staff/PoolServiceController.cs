using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SBS.Application.Features.ServiceStaff.DTOs;
using SBS.Application.Features.ServiceStaff.Queries;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SBS.Api.Controllers.Staff;

[ApiController]
[Route("api/staff/pool-services")]
public class PoolServiceController : ControllerBase
{
    private readonly ISender _sender;

    public PoolServiceController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IReadOnlyList<PoolServiceDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPoolServices([FromQuery] int poolId, [FromQuery] string? status = null)
    {
        try
        {
            var query = new GetPoolServicesQuery(poolId, status);
            var result = await _sender.Send(query);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                statusCode = 404,
                error = "Not Found",
                message = ex.Message,
                timestamp = DateTime.UtcNow
            });
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

    [HttpGet("{poolServiceId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PoolServiceDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPoolServiceDetail(int poolServiceId)
    {
        try
        {
            var query = new GetPoolServiceDetailQuery(poolServiceId);
            var result = await _sender.Send(query);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                statusCode = 404,
                error = "Not Found",
                message = ex.Message,
                timestamp = DateTime.UtcNow
            });
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
