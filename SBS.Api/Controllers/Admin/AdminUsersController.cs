using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SBS.Application.Features.Admin.Commands.ChangeUserRole;
using SBS.Application.Features.Admin.Commands.CreateManager;
using SBS.Application.Features.Admin.Commands.CreateStaff;
using SBS.Application.Features.Admin.Commands.LockUser;
using SBS.Application.Features.Admin.Commands.UnlockUser;
using SBS.Application.Features.Admin.Queries.GetRoles;
using SBS.Application.Features.Admin.Queries.GetUsers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SBS.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/users")]
public class AdminUsersController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly IValidator<CreateStaffCommand> _createStaffValidator;
    private readonly IValidator<CreateManagerCommand> _createManagerValidator;
    private readonly IValidator<ChangeUserRoleCommand> _changeRoleValidator;

    public AdminUsersController(
        ISender mediator,
        IValidator<CreateStaffCommand> createStaffValidator,
        IValidator<CreateManagerCommand> createManagerValidator,
        IValidator<ChangeUserRoleCommand> changeRoleValidator)
    {
        _mediator = mediator;
        _createStaffValidator = createStaffValidator;
        _createManagerValidator = createManagerValidator;
        _changeRoleValidator = changeRoleValidator;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _mediator.Send(new GetUsersQuery());
        return Ok(users);
    }

    [HttpPost("{userId:guid}/lock")]
    public async Task<IActionResult> LockUser(Guid userId)
    {
        var result = await _mediator.Send(new LockUserCommand(userId));
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });
        return Ok(new { message = "Khóa tài khoản thành công." });
    }

    [HttpPost("{userId:guid}/unlock")]
    public async Task<IActionResult> UnlockUser(Guid userId)
    {
        var result = await _mediator.Send(new UnlockUserCommand(userId));
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });
        return Ok(new { message = "Mở khóa tài khoản thành công." });
    }

    [HttpPost("create-staff")]
    public async Task<IActionResult> CreateStaff([FromBody] CreateStaffCommand command)
    {
        var validationResult = await _createStaffValidator.ValidateAsync(command);
        if (!validationResult.IsValid)
            return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });

        var result = await _mediator.Send(command);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });
        return Ok(new { message = "Tạo nhân viên thành công." });
    }

    [HttpPost("create-manager")]
    public async Task<IActionResult> CreateManager([FromBody] CreateManagerCommand command)
    {
        var validationResult = await _createManagerValidator.ValidateAsync(command);
        if (!validationResult.IsValid)
            return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });

        var result = await _mediator.Send(command);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });
        return Ok(new { message = "Tạo quản lý thành công." });
    }

    
}
