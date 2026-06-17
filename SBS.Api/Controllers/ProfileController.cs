using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBS.Application.Features.Users.Commands.ChangePassword;
using SBS.Application.Features.Users.Commands.UpdateAvatar;
using SBS.Application.Features.Users.Commands.UpdateProfile;
using SBS.Application.Features.Users.Queries.GetProfile;
using System.Linq;
using System.Threading.Tasks;

namespace SBS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly IValidator<UpdateProfileCommand> _updateProfileValidator;
    private readonly IValidator<ChangePasswordCommand> _changePasswordValidator;

    public ProfileController(
        ISender mediator,
        IValidator<UpdateProfileCommand> updateProfileValidator,
        IValidator<ChangePasswordCommand> changePasswordValidator)
    {
        _mediator = mediator;
        _updateProfileValidator = updateProfileValidator;
        _changePasswordValidator = changePasswordValidator;
    }


    // GET /api/profile - Lấy thông tin hồ sơ cá nhân của người dùng hiện tại.
    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var result = await _mediator.Send(new GetProfileQuery());
        if (result == null)
        {
            return NotFound(new { message = "Không tìm thấy hồ sơ cá nhân của người dùng." });
        }

        return Ok(result);
    }


    // PUT /api/profile - Cập nhật thông tin hồ sơ cá nhân.
    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileCommand command)
    {
        var validationResult = await _updateProfileValidator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { errors });
        }

        var success = await _mediator.Send(command);
        if (!success)
        {
            return BadRequest(new { message = "Cập nhật hồ sơ cá nhân thất bại." });
        }

        return Ok(new { message = "Cập nhật hồ sơ cá nhân thành công." });
    }


    // PUT /api/profile/avatar - Cập nhật đường dẫn ảnh đại diện.
    [HttpPut("avatar")]
    public async Task<IActionResult> UpdateAvatar([FromBody] UpdateAvatarCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.AvatarUrl))
        {
            return BadRequest(new { message = "Đường dẫn ảnh đại diện không được để trống." });
        }

        var success = await _mediator.Send(command);
        if (!success)
        {
            return BadRequest(new { message = "Cập nhật ảnh đại diện thất bại." });
        }

        return Ok(new { message = "Cập nhật ảnh đại diện thành công." });
    }


    // POST /api/profile/change-password - Đổi mật khẩu của tài khoản hiện tại.
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
    {
        var validationResult = await _changePasswordValidator.ValidateAsync(command);
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

        return Ok(new { message = "Thay đổi mật khẩu thành công." });
    }
}
