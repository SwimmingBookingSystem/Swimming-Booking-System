using MediatR;
using SBS.Application.Common.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Staff.Commands.CheckOut;

public record StaffCheckOutCommand : IRequest<StaffCheckOutResultDto>
{
    public int? BookingId { get; init; }
    public string? BookingCode { get; init; }
}

public record StaffCheckOutResultDto
{
    public bool Succeeded { get; init; }
    public string? Message { get; init; }
    public string? CustomerName { get; init; }
    public string? SlotTime { get; init; }
}

public class StaffCheckOutCommandHandler : IRequestHandler<StaffCheckOutCommand, StaffCheckOutResultDto>
{
    private readonly IStaffService _staffService;

    public StaffCheckOutCommandHandler(IStaffService staffService)
    {
        _staffService = staffService;
    }

    public async Task<StaffCheckOutResultDto> Handle(StaffCheckOutCommand request, CancellationToken cancellationToken)
    {
        return await _staffService.CheckOutAsync(request, cancellationToken);
    }
}
