using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SBS.Application.Features.Users.Commands.ChangePassword;
using SBS.Application.Features.Users.Commands.UpdateAvatar;
using SBS.Application.Features.Users.Commands.UpdateProfile;
using SBS.Application.Features.Users.Queries.GetProfile;
using System.Threading.Tasks;

namespace SBS.Api.Controllers.Customer;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProfileController(IMediator mediator)
    {
        _mediator = mediator;
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
        var success = await _mediator.Send(command);
        if (!success)
        {
            return BadRequest(new { message = "Cập nhật hồ sơ cá nhân thất bại." });
        }

        return Ok(new { message = "Cập nhật hồ sơ cá nhân thành công." });
    }

    // POST /api/profile/upload-avatar - Upload ảnh đại diện trực tiếp từ máy tính lên Cloudinary
    [HttpPost("upload-avatar")]
    public async Task<IActionResult> UploadAvatar(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Vui lòng chọn một file ảnh hợp lệ." });
        }

        using var stream = file.OpenReadStream();
        var avatarUrl = await _mediator.Send(new UploadAvatarCommand(stream, file.FileName));
        return Ok(new { avatarUrl, message = "Tải ảnh đại diện lên thành công." });
    }

    // POST /api/profile/change-password - Đổi mật khẩu của tài khoản hiện tại.
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.Succeeded)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return Ok(new { message = "Thay đổi mật khẩu thành công." });
    }
}
