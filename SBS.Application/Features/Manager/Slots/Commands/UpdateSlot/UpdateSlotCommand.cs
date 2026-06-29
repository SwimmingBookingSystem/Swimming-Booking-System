using FluentValidation;
using MediatR;
using SBS.Application.Common.ManagerExceptions;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.Manager.Slots.Dtos;
using SBS.Domain.Entities;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Manager.Slots.Commands.UpdateSlot;

//  Command 
public record UpdateSlotCommand(
    int SlotId,
    string? SlotName,
    TimeSpan StartTime,
    TimeSpan EndTime,
    DateOnly SlotDate,
    int Capacity
) : IRequest<PoolSlotDto>;

//  Handler 
public class UpdateSlotCommandHandler : IRequestHandler<UpdateSlotCommand, PoolSlotDto>
{
    private readonly IUnitOfWork _uow;

    public UpdateSlotCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<PoolSlotDto> Handle(UpdateSlotCommand request, CancellationToken ct)
    {
        // 1. Tìm slot
        var slot = await _uow.FirstOrDefaultAsync(
            _uow.Repository<PoolSlot>().Query().Where(s => s.PoolSlotId == request.SlotId), ct)
            ?? throw new NotFoundException(nameof(PoolSlot), request.SlotId);

        // 2. Nếu slot đã có booking → không cho sửa thời gian / ngày
        bool hasBooking = await _uow.AnyAsync(
            _uow.Repository<Booking>().Query().Where(b => b.PoolSlotId == request.SlotId), ct);

        bool timeChanged = slot.StartTime != request.StartTime
                        || slot.EndTime   != request.EndTime
                        || slot.SlotDate  != request.SlotDate;

        if (hasBooking && timeChanged)
            throw new BadRequestException(
                "Không thể thay đổi giờ hoặc ngày của slot đã có booking.");

        // 3. Validate nằm trong giờ mở cửa pool (chỉ khi thay đổi giờ)
        if (timeChanged)
        {
            var pool = await _uow.FirstOrDefaultAsync(
                _uow.Repository<Pool>().Query().Where(p => p.PoolId == slot.PoolId), ct)!;

            if (request.StartTime < pool!.OpeningTime || request.EndTime > pool.ClosingTime)
                throw new BadRequestException(
                    $"Slot phải nằm trong giờ mở cửa của bể bơi ({pool.OpeningTime:hh\\:mm} – {pool.ClosingTime:hh\\:mm}).");

            // 4. Kiểm tra trùng giờ (loại trừ chính slot này)
            bool hasOverlap = await _uow.AnyAsync(
                _uow.Repository<PoolSlot>().Query()
                    .Where(s => s.PoolId      == slot.PoolId
                             && s.PoolSlotId  != request.SlotId
                             && s.SlotDate    == request.SlotDate
                             && s.StartTime   <  request.EndTime
                             && s.EndTime     >  request.StartTime), ct);

            if (hasOverlap)
                throw new BadRequestException("Đã tồn tại slot trùng khung giờ trong bể bơi này.");
        }

        slot.SlotName  = request.SlotName;
        slot.StartTime = request.StartTime;
        slot.EndTime   = request.EndTime;
        slot.SlotDate  = request.SlotDate;
        slot.Capacity  = request.Capacity;

        _uow.Repository<PoolSlot>().Update(slot);
        await _uow.SaveChangesAsync(ct);

        return new PoolSlotDto
        {
            PoolSlotId = slot.PoolSlotId,
            PoolId     = slot.PoolId,
            SlotName   = slot.SlotName,
            StartTime  = slot.StartTime.ToString(@"hh\:mm"),
            EndTime    = slot.EndTime.ToString(@"hh\:mm"),
            SlotDate   = slot.SlotDate.ToString("yyyy-MM-dd"),
            Capacity   = slot.Capacity,
            Status     = slot.Status,
            CreatedAt  = slot.CreatedAt
        };
    }
}

// ── Validator ─────────────────────────────────────────────────────────────────
public class UpdateSlotCommandValidator : AbstractValidator<UpdateSlotCommand>
{
    public UpdateSlotCommandValidator()
    {
        RuleFor(x => x.Capacity)
            .Equal(50).WithMessage("Sức chứa chuẩn nghiệp vụ phải là đúng 50/slot.");

        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime)
            .WithMessage("Giờ kết thúc phải lớn hơn giờ bắt đầu.");
    }
}
