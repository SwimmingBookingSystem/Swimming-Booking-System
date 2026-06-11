using MediatR;
using Microsoft.AspNetCore.Mvc;
using SBS.Application.Features.Auth.Commands.Login;
using System.Threading.Tasks;

namespace SBS.Api.Controllers.Auth;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ISender _mediator;

    public AuthController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.Succeeded)
        {
            return BadRequest(new { message = result.Errors.FirstOrDefault() ?? "Tên đăng nhập hoặc mật khẩu không chính xác." });
        }

        return Ok(result.Data);
    }
}
