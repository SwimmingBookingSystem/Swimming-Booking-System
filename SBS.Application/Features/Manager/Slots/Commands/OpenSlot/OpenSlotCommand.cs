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
    private readonly IUnitOfWork _uow;

    public OpenSlotCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<SuccessResponse> Handle(OpenSlotCommand request, CancellationToken ct)
    {
        var slot = await _uow.FirstOrDefaultAsync(
            _uow.Repository<PoolSlot>().Query().Where(s => s.PoolSlotId == request.SlotId), ct)
            ?? throw new NotFoundException(nameof(PoolSlot), request.SlotId);

        if (slot.Status == "Open")
            throw new BadRequestException("Slot đang ở trạng thái Open, không cần mở lại.");

        slot.Status = "Open";
        _uow.Repository<PoolSlot>().Update(slot);
        await _uow.SaveChangesAsync(ct);

        return new SuccessResponse { Message = "Đã mở lại slot thành công." };
    }
}
