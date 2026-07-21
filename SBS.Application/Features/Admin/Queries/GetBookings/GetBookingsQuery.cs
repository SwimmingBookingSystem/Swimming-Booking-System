using MediatR;
using SBS.Application.Common.Dtos;
using SBS.Application.Common.Dtos.Admin;
using SBS.Application.Common.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Admin.Queries.GetBookings;

public record GetBookingsQuery(
    int Page = 1,
    int PageSize = 10,
    string? Status = null,
    string? Search = null,
    string? FromDate = null,
    string? ToDate = null
) : IRequest<PagedResultDto<BookingListDto>>;

public class GetBookingsQueryHandler : IRequestHandler<GetBookingsQuery, PagedResultDto<BookingListDto>>
{
    private readonly IAdminService _adminService;

    public GetBookingsQueryHandler(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public async Task<PagedResultDto<BookingListDto>> Handle(GetBookingsQuery request, CancellationToken cancellationToken)
    {
        return await _adminService.GetBookingsAsync(
            request.Page,
            request.PageSize,
            request.Status,
            request.Search,
            request.FromDate,
            request.ToDate,
            cancellationToken);
    }
}
