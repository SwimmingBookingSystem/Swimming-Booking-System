using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SBS.Application.Common.Models;
using SBS.Application.Features.ServiceStaff.Commands;
using SBS.Application.Features.ServiceStaff.DTOs;
using SBS.Application.Features.ServiceStaff.Queries;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SBS.Api.Controllers.Staff;

[ApiController]
[Route("api/staff/service-reports")]
public class ServiceReportController : ControllerBase
{
    private readonly ISender _sender;

    public ServiceReportController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ServiceReportResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateServiceReport([FromBody] CreateServiceReportRequestDto request)
    {
        try
        {
            var command = new CreateServiceReportCommand(request);
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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<ServiceReportListItemDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetServiceReports(
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var query = new GetServiceReportsQuery(status, page, pageSize);
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

    [HttpGet("{reportId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ServiceReportDetailDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetServiceReportDetail(int reportId)
    {
        try
        {
            var query = new GetServiceReportDetailQuery(reportId);
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
