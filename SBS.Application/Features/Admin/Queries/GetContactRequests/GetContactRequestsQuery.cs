using MediatR;
using SBS.Application.Common.Dtos;
using SBS.Application.Common.Dtos.Admin;
using SBS.Application.Common.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Admin.Queries.GetContactRequests;

public record GetContactRequestsQuery(int Page = 1, int PageSize = 10, string? Status = null) : IRequest<PagedResultDto<ContactRequestListDto>>;

public class GetContactRequestsQueryHandler : IRequestHandler<GetContactRequestsQuery, PagedResultDto<ContactRequestListDto>>
{
    private readonly IAdminService _adminService;

    public GetContactRequestsQueryHandler(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public async Task<PagedResultDto<ContactRequestListDto>> Handle(GetContactRequestsQuery request, CancellationToken cancellationToken)
    {
        return await _adminService.GetContactRequestsAsync(request.Page, request.PageSize, request.Status, cancellationToken);
    }
}
