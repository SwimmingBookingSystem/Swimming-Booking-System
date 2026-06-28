using MediatR;
using SBS.Application.Common.Dtos.Profile;
using SBS.Application.Common.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Admin.Commands.ChangeUserRole;

public record ChangeUserRoleCommand(Guid UserId, string Role) : IRequest<ResultDto>;

public class ChangeUserRoleCommandHandler : IRequestHandler<ChangeUserRoleCommand, ResultDto>
{
    private readonly IAdminService _adminService;

    public ChangeUserRoleCommandHandler(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public async Task<ResultDto> Handle(ChangeUserRoleCommand request, CancellationToken cancellationToken)
    {
        return await _adminService.ChangeUserRoleAsync(request.UserId, request.Role, cancellationToken);
    }
}
