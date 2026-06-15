using MediatR;
using Microsoft.AspNetCore.Mvc;
using SBS.Application.Features.Admin.Commands.LockUser;
using SBS.Application.Features.Admin.Commands.UnlockUser;
using SBS.Application.Features.Admin.Queries.GetUsers;
using System;
using System.Threading.Tasks;

namespace SBS.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/users")]
public class AdminUsersController : ControllerBase
{
    private readonly ISender _mediator;

    public AdminUsersController(ISender mediator)
    {
        _mediator = mediator;
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
}
