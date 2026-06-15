using MediatR;
using SBS.Application.Common.Dtos.Profile;
using SBS.Application.Common.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Admin.Commands.LockUser;

public record LockUserCommand(Guid UserId) : IRequest<ResultDto>;

public class LockUserCommandHandler : IRequestHandler<LockUserCommand, ResultDto>
{
    private readonly IAdminService _adminService;

    public LockUserCommandHandler(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public async Task<ResultDto> Handle(LockUserCommand request, CancellationToken cancellationToken)
    {
        return await _adminService.LockUserAsync(request.UserId, cancellationToken);
    }
}
