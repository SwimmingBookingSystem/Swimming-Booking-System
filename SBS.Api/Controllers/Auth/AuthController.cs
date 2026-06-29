using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
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
    private readonly IValidator<ForgotPasswordCommand> _forgotPasswordValidator;
    private readonly IValidator<ResetPasswordCommand> _resetPasswordValidator;

    public AuthController(
        ISender mediator, 
        IValidator<LoginCommand> loginValidator,
        IValidator<RefreshTokenCommand> refreshValidator,
        IValidator<RegisterCommand> registerValidator,
        IValidator<VerifyOtpCommand> verifyOtpValidator,
        IValidator<ResendOtpCommand> resendOtpValidator,
        IValidator<ForgotPasswordCommand> forgotPasswordValidator,
        IValidator<ResetPasswordCommand> resetPasswordValidator)
    {
        _mediator = mediator;
        _loginValidator = loginValidator;
        _refreshValidator = refreshValidator;
        _registerValidator = registerValidator;
        _verifyOtpValidator = verifyOtpValidator;
        _resendOtpValidator = resendOtpValidator;
        _forgotPasswordValidator = forgotPasswordValidator;
        _resetPasswordValidator = resetPasswordValidator;
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
        if (!result.Succeeded || result.Data == null)
        {
            return BadRequest(new { message = result.Errors.FirstOrDefault() ?? "Tên đăng nhập hoặc mật khẩu không chính xác." });
        }

        // Thiết lập Cookie HttpOnly bảo mật cho token
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true, // Luôn bật HTTPS trong môi trường chạy local và deploy
            SameSite = SameSiteMode.None, // Để cho phép chia sẻ Cookie chéo origin (từ API sang WebApp)
            Expires = result.Data.ExpiryDate
        };

        var refreshCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddDays(7)
        };

        var normalCookieOptions = new CookieOptions
        {
            HttpOnly = false, // Cookie thường để JS đọc được
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = result.Data.ExpiryDate
        };

        Response.Cookies.Append("accessToken", result.Data.AccessToken, cookieOptions);
        Response.Cookies.Append("refreshToken", result.Data.RefreshToken, refreshCookieOptions);
        Response.Cookies.Append("fullName", result.Data.FullName ?? "", normalCookieOptions);
        Response.Cookies.Append("role", result.Data.Role ?? "", normalCookieOptions);
        Response.Cookies.Append("userName", result.Data.UserName ?? "", normalCookieOptions);
        Response.Cookies.Append("userId", result.Data.Id.ToString(), normalCookieOptions);

        return Ok(new
        {
            id = result.Data.Id,
            userName = result.Data.UserName,
            fullName = result.Data.FullName,
            role = result.Data.Role,
            expiryDate = result.Data.ExpiryDate
        });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand? command = null)
    {
        string? accessToken = command?.AccessToken;
        string? refreshToken = command?.RefreshToken;

        // Nếu client không gửi trong body, đọc từ Cookies
        if (string.IsNullOrEmpty(accessToken))
        {
            accessToken = Request.Cookies["accessToken"];
        }
        if (string.IsNullOrEmpty(refreshToken))
        {
            refreshToken = Request.Cookies["refreshToken"];
        }

        if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
        {
            return BadRequest(new { message = "Không tìm thấy token trong yêu cầu hoặc Cookie." });
        }

        // Tạo command mới để gửi qua MediatR
        var refreshCommand = new RefreshTokenCommand
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };

        var validationResult = await _refreshValidator.ValidateAsync(refreshCommand);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { errors });
        }

        var result = await _mediator.Send(refreshCommand);
        if (!result.Succeeded || result.Data == null)
        {
            return BadRequest(new { message = result.Errors.FirstOrDefault() ?? "Token không hợp lệ hoặc đã hết hạn." });
        }

        // Ghi lại Cookie mới sau khi làm mới thành công
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = result.Data.ExpiryDate
        };

        var refreshCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddDays(7)
        };

        var normalCookieOptions = new CookieOptions
        {
            HttpOnly = false,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = result.Data.ExpiryDate
        };

        Response.Cookies.Append("accessToken", result.Data.AccessToken, cookieOptions);
        Response.Cookies.Append("refreshToken", result.Data.RefreshToken, refreshCookieOptions);
        Response.Cookies.Append("fullName", result.Data.FullName ?? "", normalCookieOptions);
        Response.Cookies.Append("role", result.Data.Role ?? "", normalCookieOptions);
        Response.Cookies.Append("userName", result.Data.UserName ?? "", normalCookieOptions);
        Response.Cookies.Append("userId", result.Data.Id.ToString(), normalCookieOptions);

        return Ok(new
        {
            id = result.Data.Id,
            userName = result.Data.UserName,
            fullName = result.Data.FullName,
            role = result.Data.Role,
            expiryDate = result.Data.ExpiryDate
        });
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
        var validationResult = await _forgotPasswordValidator.ValidateAsync(command);
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

        return Ok(new { message = "Mã OTP khôi phục mật khẩu đã được gửi vào Email của bạn." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
    {
        var validationResult = await _resetPasswordValidator.ValidateAsync(command);
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

        return Ok(new { message = "Đặt lại mật khẩu thành công. Vui lòng đăng nhập lại." });
    }
}
