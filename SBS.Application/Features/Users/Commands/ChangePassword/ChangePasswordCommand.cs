using MediatR;
using SBS.Application.Common.Dtos.Profile;
using SBS.Application.Common.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Users.Commands.ChangePassword;

public record ChangePasswordCommand : IRequest<ResultDto>
{
    public string? CurrentPassword { get; init; }
    public string? OldPassword { get; init; }
    public string NewPassword { get; init; } = null!;

    public string EffectiveOldPassword => !string.IsNullOrEmpty(CurrentPassword) ? CurrentPassword : (OldPassword ?? string.Empty);
}

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, ResultDto>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IIdentityService _identityService;

    public ChangePasswordCommandHandler(ICurrentUserService currentUserService, IIdentityService identityService)
    {
        _currentUserService = currentUserService;
        _identityService = identityService;
    }

    public async Task<ResultDto> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var userIdString = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return ResultDto.Failure(new[] { "Người dùng chưa đăng nhập hoặc không hợp lệ." });
        }

        return await _identityService.ChangePasswordAsync(userId, request.EffectiveOldPassword, request.NewPassword, cancellationToken);
    }
}
