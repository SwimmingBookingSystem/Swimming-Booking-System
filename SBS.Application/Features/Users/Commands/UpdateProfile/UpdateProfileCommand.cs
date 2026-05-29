using MediatR;
using SBS.Application.Common.Dtos.Profile;
using SBS.Application.Common.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Users.Commands.UpdateProfile;

public record UpdateProfileCommand : IRequest<bool>
{
    public string FullName { get; init; } = null!;
    public string? PhoneNumber { get; init; }
    public string? Address { get; init; }
    public DateOnly? Dob { get; init; }
    public string? Gender { get; init; }
}

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, bool>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IIdentityService _identityService;

    public UpdateProfileCommandHandler(ICurrentUserService currentUserService, IIdentityService identityService)
    {
        _currentUserService = currentUserService;
        _identityService = identityService;
    }

    public async Task<bool> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var userIdString = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return false;
        }

        var updateDto = new UpdateProfileDto
        {
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            Address = request.Address,
            Dob = request.Dob,
            Gender = request.Gender
        };

        return await _identityService.UpdateProfileAsync(userId, updateDto, cancellationToken);
    }
}
