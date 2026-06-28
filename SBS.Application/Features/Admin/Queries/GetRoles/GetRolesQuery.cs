using MediatR;
using SBS.Application.Common.Dtos.Admin;
using SBS.Application.Common.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Admin.Queries.GetRoles;

public record GetRolesQuery : IRequest<List<RoleDto>>;

public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, List<RoleDto>>
{
    private readonly IAdminService _adminService;

    public GetRolesQueryHandler(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public async Task<List<RoleDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        return await _adminService.GetRolesAsync(cancellationToken);
    }
}
