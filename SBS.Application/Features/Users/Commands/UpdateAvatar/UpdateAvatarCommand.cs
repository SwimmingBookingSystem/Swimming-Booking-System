using MediatR;
using SBS.Application.Common.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Users.Commands.UpdateAvatar;

public record UpdateAvatarCommand : IRequest<bool>
{
    public string AvatarUrl { get; init; } = null!;
}

public class UpdateAvatarCommandHandler : IRequestHandler<UpdateAvatarCommand, bool>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IIdentityService _identityService;

    public UpdateAvatarCommandHandler(ICurrentUserService currentUserService, IIdentityService identityService)
    {
        _currentUserService = currentUserService;
        _identityService = identityService;
    }

    public async Task<bool> Handle(UpdateAvatarCommand request, CancellationToken cancellationToken)
    {
        var userIdString = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return false;
        }

        return await _identityService.UpdateAvatarAsync(userId, request.AvatarUrl, cancellationToken);
    }
}
