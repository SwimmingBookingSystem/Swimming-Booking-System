using MediatR;
using SBS.Application.Common.Dtos;
using SBS.Application.Common.Dtos.Staff;
using SBS.Application.Common.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Staff.Queries.GetAllBookings;

public record StaffGetAllBookingsQuery : IRequest<PagedResultDto<BookingListItemDto>>
{
    public string? Status { get; init; }
    public int? PoolId { get; init; }
    public string? BookingType { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public class StaffGetAllBookingsQueryHandler : IRequestHandler<StaffGetAllBookingsQuery, PagedResultDto<BookingListItemDto>>
{
    private readonly IStaffService _staffService;

    public StaffGetAllBookingsQueryHandler(IStaffService staffService)
    {
        _staffService = staffService;
    }

    public async Task<PagedResultDto<BookingListItemDto>> Handle(StaffGetAllBookingsQuery request, CancellationToken cancellationToken)
    {
        return await _staffService.GetAllBookingsAsync(request, cancellationToken);
    }
}
