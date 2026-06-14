using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SBS.Application.Features.Auth.Commands.Login;
using System.Linq;
using System.Threading.Tasks;

namespace SBS.Api.Controllers.Auth;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly IValidator<LoginCommand> _loginValidator;

    public AuthController(ISender mediator, IValidator<LoginCommand> loginValidator)
    {
        _mediator = mediator;
        _loginValidator = loginValidator;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var validationResult = await _loginValidator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { errors });
        }

        var result = await _mediator.Send(command);
        if (!result.Succeeded)
        {
            return BadRequest(new { message = result.Errors.FirstOrDefault() ?? "Tên đăng nhập hoặc mật khẩu không chính xác." });
        }

        return Ok(result.Data);
    }
}
