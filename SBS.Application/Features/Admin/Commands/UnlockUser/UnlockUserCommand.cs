using MediatR;
using SBS.Application.Common.Dtos.Profile;
using SBS.Application.Common.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Admin.Commands.UnlockUser;

public record UnlockUserCommand(Guid UserId) : IRequest<ResultDto>;

public class UnlockUserCommandHandler : IRequestHandler<UnlockUserCommand, ResultDto>
{
    private readonly IAdminService _adminService;

    public UnlockUserCommandHandler(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public async Task<ResultDto> Handle(UnlockUserCommand request, CancellationToken cancellationToken)
    {
        return await _adminService.UnlockUserAsync(request.UserId, cancellationToken);
    }
}
