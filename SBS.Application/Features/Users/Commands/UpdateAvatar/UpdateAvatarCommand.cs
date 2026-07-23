using MediatR;
using SBS.Application.Common.Interfaces;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Users.Commands.UpdateAvatar;

public record UploadAvatarCommand(Stream FileStream, string FileName) : IRequest<string>;

public class UploadAvatarCommandHandler : IRequestHandler<UploadAvatarCommand, string>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IIdentityService _identityService;
    private readonly ICloudinaryService _cloudinaryService;

    public UploadAvatarCommandHandler(
        ICurrentUserService currentUserService,
        IIdentityService identityService,
        ICloudinaryService cloudinaryService)
    {
        _currentUserService = currentUserService;
        _identityService = identityService;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<string> Handle(UploadAvatarCommand request, CancellationToken cancellationToken)
    {
        var userIdString = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            throw new UnauthorizedAccessException("Bạn chưa đăng nhập.");
        }

        var avatarUrl = await _cloudinaryService.UploadImageAsync(request.FileStream, request.FileName, "avatars");
        var success = await _identityService.UpdateAvatarAsync(userId, avatarUrl, cancellationToken);
        if (!success)
        {
            throw new InvalidOperationException("Cập nhật ảnh đại diện thất bại.");
        }

        return avatarUrl;
    }
}
