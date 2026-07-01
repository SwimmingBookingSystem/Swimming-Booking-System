using MediatR;
using SBS.Application.Common.Dtos.Admin;
using SBS.Application.Common.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Admin.Queries.GetAdminPools;

public record GetAdminPoolsQuery : IRequest<List<AdminPoolDto>>;

public class GetAdminPoolsQueryHandler : IRequestHandler<GetAdminPoolsQuery, List<AdminPoolDto>>
{
    private readonly IAdminService _adminService;

    public GetAdminPoolsQueryHandler(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public async Task<List<AdminPoolDto>> Handle(GetAdminPoolsQuery request, CancellationToken cancellationToken)
    {
        return await _adminService.GetPoolsAsync(cancellationToken);
    }
}
