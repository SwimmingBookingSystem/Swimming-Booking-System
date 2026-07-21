using MediatR;
using SBS.Application.Common.Dtos.Manager;
using SBS.Application.Common.ManagerExceptions;
using SBS.Application.Common.Interfaces;
using SBS.Domain.Entities;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Manager.Slots.Commands.OpenSlot;

//  Command
public record OpenSlotCommand(int SlotId) : IRequest<SuccessResponse>;

//  Handler
public class OpenSlotCommandHandler : IRequestHandler<OpenSlotCommand, SuccessResponse>
{
    private readonly SBS.Application.Features.Manager.Services.Interfaces.ISlotManagementService _slotService;

    public OpenSlotCommandHandler(SBS.Application.Features.Manager.Services.Interfaces.ISlotManagementService slotService) => _slotService = slotService;

    public async Task<SuccessResponse> Handle(OpenSlotCommand request, CancellationToken ct)
    {
        return await _slotService.OpenSlotAsync(request.SlotId, ct);
    }
}

