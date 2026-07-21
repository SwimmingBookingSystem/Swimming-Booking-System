using MediatR;
using SBS.Application.Common.Dtos.Staff;
using SBS.Application.Common.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Staff.Commands.QrCheckIn;

public record StaffQrCheckInCommand : IRequest<StaffCheckInResultDto>
{
    public string BookingCode { get; init; } = null!;
}

public class StaffQrCheckInCommandHandler : IRequestHandler<StaffQrCheckInCommand, StaffCheckInResultDto>
{
    private readonly IStaffService _staffService;

    public StaffQrCheckInCommandHandler(IStaffService staffService)
    {
        _staffService = staffService;
    }

    public async Task<StaffCheckInResultDto> Handle(StaffQrCheckInCommand request, CancellationToken cancellationToken)
    {
        return await _staffService.QrCheckInAsync(request, cancellationToken);
    }
}
