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

        var pool = await _uow.FirstOrDefaultAsync(
            _uow.Repository<Pool>().Query().Where(p => p.PoolId == slot.PoolId), ct)!;

        if (request.Capacity < 1 || request.Capacity > pool.StandardCapacity)
            throw new BadRequestException($"Sức chứa ca bơi phải lớn hơn 0 và không vượt quá giới hạn an toàn của bể bơi ({pool.StandardCapacity} người).");

        slot.Capacity = request.Capacity;
        _uow.Repository<PoolSlot>().Update(slot);
        await _uow.SaveChangesAsync(ct);

        return new SuccessResponse { Message = $"Đã cập nhật sức chứa thành {request.Capacity}." };
    }
}
