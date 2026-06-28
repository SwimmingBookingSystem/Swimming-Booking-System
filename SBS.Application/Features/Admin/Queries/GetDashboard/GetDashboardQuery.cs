using MediatR;
using SBS.Application.Common.Dtos.Admin;
using SBS.Application.Common.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Admin.Queries.GetDashboard;

public record GetDashboardQuery : IRequest<DashboardDto>;

public class GetDashboardQueryHandler : IRequestHandler<GetDashboardQuery, DashboardDto>
{
    private readonly IAdminService _adminService;

    public GetDashboardQueryHandler(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public async Task<DashboardDto> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        return await _adminService.GetDashboardAsync(cancellationToken);
    }
}
