using FluentValidation;
using MediatR;
using SBS.Application.Common.Dtos.Manager;
using SBS.Application.Common.ManagerExceptions;
using SBS.Application.Common.Interfaces;
using SBS.Domain.Entities;
using System.Linq;
using Microsoft.EntityFrameworkCore;
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

        // Đếm tổng số lượng vé đã được đặt cho slot này
        int currentBooked = await _uow.Repository<BookingDetail>().Query()
            .Where(bd => bd.Booking.PoolSlotId == request.SlotId && bd.Booking.Status != "Cancelled" && bd.Booking.Status != "Failed")
            .SumAsync(bd => bd.Quantity, ct);

        // Phương án 1: Chặn cứng (Hard Limit)
        if (request.Capacity < currentBooked)
        {
            throw new BadRequestException($"Không thể giảm tổng sức chứa xuống {request.Capacity} vì hiện tại đã có {currentBooked} vé được đặt. Sức chứa tối thiểu cho phép là {currentBooked}.");
        }

        slot.Capacity = request.Capacity - currentBooked;
        _uow.Repository<PoolSlot>().Update(slot);
        await _uow.SaveChangesAsync(ct);

        return new SuccessResponse { Message = $"Đã cập nhật sức chứa thành {request.Capacity}." };
    }
}

// Validator 
public class UpdateSlotCapacityCommandValidator : AbstractValidator<UpdateSlotCapacityCommand>
{
    public UpdateSlotCapacityCommandValidator()
    {
        RuleFor(x => x.Capacity).GreaterThan(0).WithMessage("Sức chứa phải lớn hơn 0.");
    }
}
