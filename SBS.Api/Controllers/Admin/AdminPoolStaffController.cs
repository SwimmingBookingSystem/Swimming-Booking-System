using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBS.Application.Features.Admin.Commands.AssignStaffToPool;
using SBS.Application.Features.Admin.Commands.UnassignStaffFromPool;
using SBS.Application.Features.Admin.Queries.GetStaffAssignments;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SBS.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/pool-staff")]
[Authorize(Roles = "Admin")]
public class AdminPoolStaffController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly IValidator<AssignStaffToPoolCommand> _assignValidator;
    private readonly IValidator<UnassignStaffFromPoolCommand> _unassignValidator;

    public AdminPoolStaffController(
        ISender mediator,
        IValidator<AssignStaffToPoolCommand> assignValidator,
        IValidator<UnassignStaffFromPoolCommand> unassignValidator)
    {
        _mediator = mediator;
        _assignValidator = assignValidator;
        _unassignValidator = unassignValidator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAssignments(
        [FromQuery] Guid? staffId,
        [FromQuery] int? poolId)
    {
        var result = await _mediator.Send(new GetStaffAssignmentsQuery
        {
            StaffId = staffId,
            PoolId = poolId
        });
        return Ok(result);
    }

    [HttpPost("assign")]
    public async Task<IActionResult> Assign([FromBody] AssignStaffToPoolCommand command)
    {
        var validationResult = await _assignValidator.ValidateAsync(command);
        if (!validationResult.IsValid)
            return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });

        var result = await _mediator.Send(command);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(new { message = "Phân công Staff vào hồ bơi thành công." });
    }

    [HttpPost("unassign")]
    public async Task<IActionResult> Unassign([FromBody] UnassignStaffFromPoolCommand command)
    {
        var validationResult = await _unassignValidator.ValidateAsync(command);
        if (!validationResult.IsValid)
            return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });

        var result = await _mediator.Send(command);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(new { message = "Thu hồi phân công thành công." });
    }
}
