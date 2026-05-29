using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SBS.Application.Features.CheckIn.Commands;
using SBS.Application.Features.CheckIn.DTOs;
using SBS.Application.Features.CheckIn.Queries;
using System;
using System.Threading.Tasks;

namespace SBS.Api.Controllers.Staff;

[ApiController]
[Route("api/staff/checkin")]
public class CheckInController : ControllerBase
{
    private readonly ISender _sender;

    public CheckInController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("verify")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VerifyTicketResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Verify([FromBody] VerifyTicketRequestDto request)
    {
        try
        {
            var query = new VerifyTicketQuery(request.TicketCode);
            var result = await _sender.Send(query);
            
            if (!result.IsValid && result.Reason == "Mã vé không tồn tại trong hệ thống.")
            {
                return NotFound(new
                {
                    statusCode = 404,
                    error = "Not Found",
                    message = result.Reason,
                    timestamp = DateTime.UtcNow
                });
            }

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

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CheckInResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CheckIn([FromBody] CheckInRequestDto request)
    {
        try
        {
            var command = new CheckInCommand(request.TicketCode, request.BookingId, request.StaffId);
            var result = await _sender.Send(command);
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("đã check-in"))
        {
            return StatusCode(StatusCodes.Status409Conflict, new
            {
                statusCode = 409,
                error = "Conflict",
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
