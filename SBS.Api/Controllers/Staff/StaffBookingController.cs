using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBS.Application.Features.Staff.Queries.GetAllBookings;
using SBS.Application.Features.Staff.Queries.GetBookingDetail;
using SBS.Application.Features.Staff.Queries.SearchBookings;
using System;
using System.Threading.Tasks;

namespace SBS.Api.Controllers.Staff;

/// <summary>
/// Quản lý booking - dành cho nhân viên lễ tân.
/// </summary>
[ApiController]
[Route("api/staff/bookings")]
[Authorize(Roles = "Staff,Manager,Admin")]
public class StaffBookingController : ControllerBase
{
    private readonly ISender _mediator;

    public StaffBookingController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// GET /api/staff/bookings?status=Confirmed&bookingDate=2025-06-10&poolId=1&page=1&pageSize=20
    /// Xem tất cả booking với filter và phân trang.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllBookings(
        [FromQuery] string? status,
        [FromQuery] string? bookingDate,
        [FromQuery] int? poolId,
        [FromQuery] string? bookingType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        DateOnly? parsedDate = null;
        if (!string.IsNullOrWhiteSpace(bookingDate) && DateOnly.TryParse(bookingDate, out var d))
            parsedDate = d;

        var query = new StaffGetAllBookingsQuery
        {
            Status = status,
            PoolId = poolId,
            BookingType = bookingType,
            Page = Math.Max(1, page),
            PageSize = Math.Clamp(pageSize, 1, 100)
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// GET /api/staff/bookings/search?bookingCode=SBS-XXX&phone=0912345678&email=abc@gmail.com
    /// Tìm kiếm booking theo mã, số điện thoại hoặc email.
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> SearchBookings(
        [FromQuery] string? bookingCode,
        [FromQuery] string? phone,
        [FromQuery] string? email)
    {
        if (string.IsNullOrWhiteSpace(bookingCode) &&
            string.IsNullOrWhiteSpace(phone) &&
            string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new { message = "Vui lòng nhập ít nhất một tiêu chí tìm kiếm (mã booking, số điện thoại hoặc email)." });
        }

        var query = new StaffSearchBookingsQuery
        {
            BookingCode = bookingCode,
            Phone = phone,
            Email = email
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// GET /api/staff/bookings/{id}
    /// Xem chi tiết một booking cụ thể.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetBookingDetail([FromRoute] int id)
    {
        var result = await _mediator.Send(new StaffGetBookingDetailQuery { BookingId = id });
        if (result is null)
            return NotFound(new { message = $"Không tìm thấy booking với id = {id}." });

        return Ok(result);
    }
}

