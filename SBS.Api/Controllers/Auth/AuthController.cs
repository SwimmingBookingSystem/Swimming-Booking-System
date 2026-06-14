using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SBS.Application.Features.Auth.Commands.Login;
using SBS.Application.Features.Auth.Commands.RefreshToken;
using System.Linq;
using System.Threading.Tasks;

namespace SBS.Api.Controllers.Auth;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly IValidator<LoginCommand> _loginValidator;
    private readonly IValidator<RefreshTokenCommand> _refreshValidator;

    public AuthController(
        ISender mediator, 
        IValidator<LoginCommand> loginValidator,
        IValidator<RefreshTokenCommand> refreshValidator)
    {
        _mediator = mediator;
        _loginValidator = loginValidator;
        _refreshValidator = refreshValidator;
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

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command)
    {
        var validationResult = await _refreshValidator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { errors });
        }

        var result = await _mediator.Send(command);
        if (!result.Succeeded)
        {
            return BadRequest(new { message = result.Errors.FirstOrDefault() ?? "Token không hợp lệ hoặc đã hết hạn." });
        }

        return Ok(result.Data);
    }
}
