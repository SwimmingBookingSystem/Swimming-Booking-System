using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBS.Application.Features.Staff.Commands.ManualCheckIn;
using SBS.Application.Features.Staff.Commands.QrCheckIn;
using System.Linq;
using System.Threading.Tasks;

namespace SBS.Api.Controllers.Staff;

/// <summary>
/// Quản lý check-in khách bơi - dành cho nhân viên lễ tân.
/// </summary>
[ApiController]
[Route("api/staff/checkin")]
[Authorize(Roles = "Staff,Manager,Admin")]
public class StaffCheckInController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly IValidator<StaffQrCheckInCommand> _qrCheckInValidator;
    private readonly IValidator<StaffManualCheckInCommand> _manualCheckInValidator;

    public StaffCheckInController(
        ISender mediator,
        IValidator<StaffQrCheckInCommand> qrCheckInValidator,
        IValidator<StaffManualCheckInCommand> manualCheckInValidator)
    {
        _mediator = mediator;
        _qrCheckInValidator = qrCheckInValidator;
        _manualCheckInValidator = manualCheckInValidator;
    }

    /// <summary>
    /// POST /api/staff/checkin/qr
    /// Check-in khách bằng QR code (quét mã BookingCode từ QR).
    /// </summary>
    [HttpPost("qr")]
    public async Task<IActionResult> QrCheckIn([FromBody] StaffQrCheckInCommand command)
    {
        var validationResult = await _qrCheckInValidator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { errors });
        }

        var result = await _mediator.Send(command);
        if (!result.Succeeded)
            return BadRequest(new { message = result.Message });

        return Ok(new
        {
            message = result.Message,
            customerName = result.CustomerName,
            slotTime = result.SlotTime
        });
    }

    /// <summary>
    /// POST /api/staff/checkin/manual
    /// Check-in thủ công khi QR bị lỗi hoặc khách không có QR (dùng BookingId).
    /// </summary>
    [HttpPost("manual")]
    public async Task<IActionResult> ManualCheckIn([FromBody] StaffManualCheckInCommand command)
    {
        var validationResult = await _manualCheckInValidator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { errors });
        }

        var result = await _mediator.Send(command);
        if (!result.Succeeded)
            return BadRequest(new { message = result.Message });

        return Ok(new
        {
            message = result.Message,
            customerName = result.CustomerName,
            slotTime = result.SlotTime
        });
    }
}
