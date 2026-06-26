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
    private readonly IUnitOfWork _uow;

    public CloseSlotCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<SuccessResponse> Handle(CloseSlotCommand request, CancellationToken ct)
    {
        var slot = await _uow.FirstOrDefaultAsync(
            _uow.Repository<PoolSlot>().Query().Where(s => s.PoolSlotId == request.SlotId), ct)
            ?? throw new NotFoundException(nameof(PoolSlot), request.SlotId);

        if (slot.Status == "Closed")
            throw new BadRequestException("Slot đã ở trạng thái Closed.");

        slot.Status = "Closed";
        _uow.Repository<PoolSlot>().Update(slot);
        await _uow.SaveChangesAsync(ct);

        return new SuccessResponse { Message = "Đã đóng slot thành công." };
    }
}
