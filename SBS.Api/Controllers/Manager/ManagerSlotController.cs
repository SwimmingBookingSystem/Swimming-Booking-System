using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBS.Application.Features.Manager.Slots.Commands.CloseSlot;
using SBS.Application.Features.Manager.Slots.Commands.CreateSlot;
using SBS.Application.Features.Manager.Slots.Commands.OpenSlot;
using SBS.Application.Features.Manager.Slots.Commands.UpdateSlot;
using SBS.Application.Features.Manager.Slots.Commands.UpdateSlotCapacity;
using SBS.Application.Features.Manager.Slots.Queries.GetSlotById;
using SBS.Application.Features.Manager.Slots.Queries.GetSlotsByPool;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SBS.Api.Controllers.Manager;

[ApiController]
[Route("api/manager")]
[Authorize(Roles = "Manager")]
public class ManagerSlotController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly IValidator<CreateSlotCommand> _createValidator;
    private readonly IValidator<UpdateSlotCommand> _updateValidator;
    private readonly IValidator<UpdateSlotCapacityCommand> _capacityValidator;

    public ManagerSlotController(
        ISender mediator,
        IValidator<CreateSlotCommand> createValidator,
        IValidator<UpdateSlotCommand> updateValidator,
        IValidator<UpdateSlotCapacityCommand> capacityValidator)
    {
        _mediator          = mediator;
        _createValidator   = createValidator;
        _updateValidator   = updateValidator;
        _capacityValidator = capacityValidator;
    }

    /// Lấy danh sách slot theo bể bơi (có phân trang, lọc theo ngày/status)
    [HttpGet("pools/{poolId:int}/slots")]
    public async Task<IActionResult> GetSlots(
        int poolId,
        [FromQuery] int page      = 1,
        [FromQuery] int pageSize  = 10,
        [FromQuery] string? date   = null,
        [FromQuery] string? status = null)
        => Ok(await _mediator.Send(new GetSlotsByPoolQuery(poolId, page, pageSize, date, status)));

    /// Lấy chi tiết slot theo ID
    [HttpGet("slots/{slotId:int}")]
    public async Task<IActionResult> GetSlot(int slotId)
        => Ok(await _mediator.Send(new GetSlotByIdQuery(slotId)));

    /// Tạo slot mới cho bể bơi
    [HttpPost("pools/{poolId:int}/slots")]
    public async Task<IActionResult> CreateSlot(int poolId, [FromBody] CreatePoolSlotRequest request)
    {
        if (!TimeSpan.TryParse(request.StartTime, out var start))
            return BadRequest(new { message = "StartTime không hợp lệ. Định dạng: HH:mm" });

        if (!TimeSpan.TryParse(request.EndTime, out var end))
            return BadRequest(new { message = "EndTime không hợp lệ. Định dạng: HH:mm" });

        if (!DateOnly.TryParse(request.SlotDate, out var slotDate))
            return BadRequest(new { message = "SlotDate không hợp lệ. Định dạng: yyyy-MM-dd" });

        var command = new CreateSlotCommand(poolId, request.SlotName, start, end, slotDate, request.Capacity);

        var validation = await _createValidator.ValidateAsync(command);
        if (!validation.IsValid)
            return BadRequest(new { errors = validation.Errors.Select(e => e.ErrorMessage) });

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetSlot), new { slotId = result.PoolSlotId }, result);
    }

    /// Cập nhật thông tin slot
    [HttpPut("slots/{slotId:int}")]
    public async Task<IActionResult> UpdateSlot(int slotId, [FromBody] UpdatePoolSlotRequest request)
    {
        if (!TimeSpan.TryParse(request.StartTime, out var start))
            return BadRequest(new { message = "StartTime không hợp lệ. Định dạng: HH:mm" });

        if (!TimeSpan.TryParse(request.EndTime, out var end))
            return BadRequest(new { message = "EndTime không hợp lệ. Định dạng: HH:mm" });

        if (!DateOnly.TryParse(request.SlotDate, out var slotDate))
            return BadRequest(new { message = "SlotDate không hợp lệ. Định dạng: yyyy-MM-dd" });

        var command = new UpdateSlotCommand(slotId, request.SlotName, start, end, slotDate, request.Capacity);

        var validation = await _updateValidator.ValidateAsync(command);
        if (!validation.IsValid)
            return BadRequest(new { errors = validation.Errors.Select(e => e.ErrorMessage) });

        return Ok(await _mediator.Send(command));
    }

    /// Đóng slot
    [HttpPatch("slots/{slotId:int}/close")]
    public async Task<IActionResult> CloseSlot(int slotId)
        => Ok(await _mediator.Send(new CloseSlotCommand(slotId)));

    /// Mở lại slot đã đóng
    [HttpPatch("slots/{slotId:int}/open")]
    public async Task<IActionResult> OpenSlot(int slotId)
        => Ok(await _mediator.Send(new OpenSlotCommand(slotId)));

    /// Cập nhật sức chứa slot
    [HttpPatch("slots/{slotId:int}/capacity")]
    public async Task<IActionResult> UpdateCapacity(int slotId, [FromBody] UpdateSlotCapacityRequest request)
    {
        var command    = new UpdateSlotCapacityCommand(slotId, request.Capacity);
        var validation = await _capacityValidator.ValidateAsync(command);
        if (!validation.IsValid)
            return BadRequest(new { errors = validation.Errors.Select(e => e.ErrorMessage) });

        return Ok(await _mediator.Send(command));
    }
    
    /// Tự động sinh slot hàng loạt
    [HttpPost("pools/{poolId:int}/slots/generate")]
    public async Task<IActionResult> GenerateSlots(int poolId, [FromBody] GenerateSlotsRequest request)
    {
        if (!DateOnly.TryParse(request.StartDate, out var start))
            return BadRequest(new { message = "StartDate không hợp lệ. Định dạng: yyyy-MM-dd" });

        if (!DateOnly.TryParse(request.EndDate, out var end))
            return BadRequest(new { message = "EndDate không hợp lệ. Định dạng: yyyy-MM-dd" });

        var command = new SBS.Application.Features.Manager.Slots.Commands.GenerateSlots.GenerateSlotsCommand(
            poolId, start, end, request.DurationMinutes, request.BreakMinutes);

        var validator = new SBS.Application.Features.Manager.Slots.Commands.GenerateSlots.GenerateSlotsCommandValidator();
        var validation = await validator.ValidateAsync(command);
        if (!validation.IsValid)
            return BadRequest(new { errors = validation.Errors.Select(e => e.ErrorMessage) });

        return Ok(await _mediator.Send(command));
    }
}

// Request Models
public class CreatePoolSlotRequest
{
    public string? SlotName { get; set; }
    public string StartTime { get; set; } = null!;  // "HH:mm"
    public string EndTime { get; set; } = null!;    // "HH:mm"
    public string SlotDate { get; set; } = null!;   // "yyyy-MM-dd"
    public int Capacity { get; set; }
}

public class UpdatePoolSlotRequest
{
    public string? SlotName { get; set; }
    public string StartTime { get; set; } = null!;
    public string EndTime { get; set; } = null!;
    public string SlotDate { get; set; } = null!;
    public int Capacity { get; set; }
}

public class UpdateSlotCapacityRequest
{
    public int Capacity { get; set; }
}

public class GenerateSlotsRequest
{
    public string StartDate { get; set; } = null!;
    public string EndDate { get; set; } = null!;
    public int DurationMinutes { get; set; }
    public int BreakMinutes { get; set; }
}
