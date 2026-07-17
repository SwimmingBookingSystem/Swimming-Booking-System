using MediatR;
using SBS.Application.Common.Dtos.Admin;
using SBS.Application.Common.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Admin.Queries.GetUsers;

public record GetUsersQuery : IRequest<List<UserListDto>>;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, List<UserListDto>>
{
    private readonly IAdminService _adminService;

    public GetUsersQueryHandler(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public async Task<List<UserListDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        return await _adminService.GetUsersAsync(cancellationToken);
    }
}
