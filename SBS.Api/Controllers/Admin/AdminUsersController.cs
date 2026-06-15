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

    
}
