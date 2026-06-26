using FluentValidation;
using MediatR;
using SBS.Application.Common.Dtos.Manager;
using SBS.Application.Common.ManagerExceptions;
using SBS.Application.Common.Interfaces;
using SBS.Domain.Entities;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Manager.Slots.Commands.UpdateSlotCapacity;

//  Command 
public record UpdateSlotCapacityCommand(int SlotId, int Capacity) : IRequest<SuccessResponse>;

//  Handler 
public class UpdateSlotCapacityCommandHandler
    : IRequestHandler<UpdateSlotCapacityCommand, SuccessResponse>
{
    private readonly IUnitOfWork _uow;

    public UpdateSlotCapacityCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<SuccessResponse> Handle(UpdateSlotCapacityCommand request, CancellationToken ct)
    {
        var slot = await _uow.FirstOrDefaultAsync(
            _uow.Repository<PoolSlot>().Query().Where(s => s.PoolSlotId == request.SlotId), ct)
            ?? throw new NotFoundException(nameof(PoolSlot), request.SlotId);

        slot.Capacity = request.Capacity;
        _uow.Repository<PoolSlot>().Update(slot);
        await _uow.SaveChangesAsync(ct);

        return new SuccessResponse { Message = $"Đã cập nhật sức chứa thành {request.Capacity}." };
    }
}

// ── Validator ─────────────────────────────────────────────────────────────────
public class UpdateSlotCapacityCommandValidator : AbstractValidator<UpdateSlotCapacityCommand>
{
    public UpdateSlotCapacityCommandValidator()
    {
        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Sức chứa phải lớn hơn 0.");
    }
}
