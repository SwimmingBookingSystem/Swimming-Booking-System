using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SBS.Application.Features.Auth.Commands.Login;
using SBS.Application.Features.Auth.Commands.RefreshToken;
using SBS.Application.Features.Auth.Commands.Register;
using SBS.Application.Features.Auth.Commands.VerifyOtp;
using SBS.Application.Features.Auth.Commands.ResendOtp;
using SBS.Application.Features.Auth.Commands.ForgotPassword;
using SBS.Application.Features.Auth.Commands.ResetPassword;
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
    private readonly IValidator<RegisterCommand> _registerValidator;
    private readonly IValidator<VerifyOtpCommand> _verifyOtpValidator;
    private readonly IValidator<ResendOtpCommand> _resendOtpValidator;

    public AuthController(
        ISender mediator, 
        IValidator<LoginCommand> loginValidator,
        IValidator<RefreshTokenCommand> refreshValidator,
        IValidator<RegisterCommand> registerValidator,
        IValidator<VerifyOtpCommand> verifyOtpValidator,
        IValidator<ResendOtpCommand> resendOtpValidator)
    {
        _mediator = mediator;
        _loginValidator = loginValidator;
        _refreshValidator = refreshValidator;
        _registerValidator = registerValidator;
        _verifyOtpValidator = verifyOtpValidator;
        _resendOtpValidator = resendOtpValidator;
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

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand? command)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(new { errors });
        }

        if (command == null)
        {
            return BadRequest(new { errors = new[] { "Dữ liệu đăng ký không hợp lệ." } });
        }

        var validationResult = await _registerValidator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { errors });
        }

        var result = await _mediator.Send(command);
        if (!result.Succeeded)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return Ok(new { message = "Vui lòng nhập OTP để tiếp tục" });
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpCommand command)
    {
        var validationResult = await _verifyOtpValidator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { errors });
        }

        var result = await _mediator.Send(command);
        if (!result.Succeeded)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return Ok(new { message = "Đăng ký tài khoản thành công." });
    }

    [HttpPost("resend-otp")]
    public async Task<IActionResult> ResendOtp([FromBody] ResendOtpCommand command)
    {
        var validationResult = await _resendOtpValidator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { errors });
        }

        var result = await _mediator.Send(command);
        if (!result.Succeeded)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return Ok(new { message = "Mã OTP mới đã được gửi vào Email của bạn." });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.Succeeded)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return Ok(new { message = "Mã OTP khôi phục mật khẩu đã được gửi vào Email của bạn." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.Succeeded)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return Ok(new { message = "Đặt lại mật khẩu thành công. Vui lòng đăng nhập lại." });
    }
}
