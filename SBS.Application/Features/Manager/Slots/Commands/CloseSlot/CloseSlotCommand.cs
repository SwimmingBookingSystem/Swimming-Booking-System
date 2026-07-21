using MediatR;
using SBS.Application.Common.Dtos.Manager;
using SBS.Application.Common.ManagerExceptions;
using SBS.Application.Common.Interfaces;
using SBS.Domain.Entities;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Manager.Slots.Commands.CloseSlot;

//  Command 
public record CloseSlotCommand(int SlotId) : IRequest<SuccessResponse>;

//  Handler 
public class CloseSlotCommandHandler : IRequestHandler<CloseSlotCommand, SuccessResponse>
{
    private readonly SBS.Application.Features.Manager.Services.Interfaces.ISlotManagementService _slotService;

    public CloseSlotCommandHandler(SBS.Application.Features.Manager.Services.Interfaces.ISlotManagementService slotService) => _slotService = slotService;

    public async Task<SuccessResponse> Handle(CloseSlotCommand request, CancellationToken ct)
    {
        return await _slotService.CloseSlotAsync(request.SlotId, ct);
    }
}

