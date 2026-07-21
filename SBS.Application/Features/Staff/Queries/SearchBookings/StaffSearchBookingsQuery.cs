using MediatR;
using SBS.Application.Common.Dtos.Staff;
using SBS.Application.Common.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Staff.Queries.SearchBookings;

public record StaffSearchBookingsQuery : IRequest<List<BookingListItemDto>>
{
    public string? BookingCode { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
}

public class StaffSearchBookingsQueryHandler : IRequestHandler<StaffSearchBookingsQuery, List<BookingListItemDto>>
{
    private readonly IStaffService _staffService;

    public StaffSearchBookingsQueryHandler(IStaffService staffService)
    {
        _staffService = staffService;
    }

    public async Task<List<BookingListItemDto>> Handle(StaffSearchBookingsQuery request, CancellationToken cancellationToken)
    {
        return await _staffService.SearchBookingsAsync(request, cancellationToken);
    }
}
