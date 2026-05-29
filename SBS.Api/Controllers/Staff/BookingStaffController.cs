using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SBS.Application.Common.Models;
using SBS.Application.Features.CheckIn.DTOs;
using SBS.Application.Features.CheckIn.Queries;
using SBS.Application.Features.ServiceStaff.DTOs;
using SBS.Application.Features.ServiceStaff.Queries;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SBS.Api.Controllers.Staff;

[ApiController]
[Route("api/staff/bookings")]
public class BookingStaffController : ControllerBase
{
    private readonly ISender _sender;

    public BookingStaffController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<BookingListItemDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetBookings(
        [FromQuery] int poolId,
        [FromQuery] DateOnly? date = null,
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var query = new GetBookingsQuery(poolId, date, status, page, pageSize);
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

    [HttpGet("{bookingId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookingDetailDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBookingDetail(int bookingId)
    {
        try
        {
            var query = new GetBookingDetailQuery(bookingId);
            var result = await _sender.Send(query);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new
            {
                statusCode = 404,
                error = "Not Found",
                message = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }

    [HttpGet("{bookingId:int}/services")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookingServicesDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBookingServices(int bookingId)
    {
        try
        {
            var query = new GetBookingServicesQuery(bookingId);
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
    }
}
