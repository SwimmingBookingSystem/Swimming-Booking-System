using MediatR;
using SBS.Application.Common.Dtos.Staff;
using SBS.Application.Common.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Staff.Commands.ManualCheckIn;

public record StaffManualCheckInCommand : IRequest<StaffCheckInResultDto>
{
    public int? BookingId { get; init; }
    public string? BookingCode { get; init; }
}

public class StaffManualCheckInCommandHandler : IRequestHandler<StaffManualCheckInCommand, StaffCheckInResultDto>
{
    private readonly IStaffService _staffService;

    public StaffManualCheckInCommandHandler(IStaffService staffService)
    {
        _staffService = staffService;
    }

    public async Task<StaffCheckInResultDto> Handle(StaffManualCheckInCommand request, CancellationToken cancellationToken)
    {
        return await _staffService.ManualCheckInAsync(request, cancellationToken);
    }
}
