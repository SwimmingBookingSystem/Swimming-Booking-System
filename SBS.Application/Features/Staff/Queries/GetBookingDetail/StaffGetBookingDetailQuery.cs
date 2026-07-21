using MediatR;
using SBS.Application.Common.Dtos.Staff;
using SBS.Application.Common.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Staff.Queries.GetBookingDetail;

public record StaffGetBookingDetailQuery : IRequest<BookingDetailDto?>
{
    public int BookingId { get; init; }
}

public class StaffGetBookingDetailQueryHandler : IRequestHandler<StaffGetBookingDetailQuery, BookingDetailDto?>
{
    private readonly IStaffService _staffService;

    public StaffGetBookingDetailQueryHandler(IStaffService staffService)
    {
        _staffService = staffService;
    }

    public async Task<BookingDetailDto?> Handle(StaffGetBookingDetailQuery request, CancellationToken cancellationToken)
    {
        return await _staffService.GetBookingDetailAsync(request, cancellationToken);
    }
}
