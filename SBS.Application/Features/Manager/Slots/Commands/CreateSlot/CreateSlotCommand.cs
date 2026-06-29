using FluentValidation;
using MediatR;
using SBS.Application.Common.ManagerExceptions;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.Manager.Slots.Dtos;
using SBS.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Manager.Slots.Commands.CreateSlot;

// Command
public record CreateSlotCommand(
    int PoolId,
    string? SlotName,
    TimeSpan StartTime,
    TimeSpan EndTime,
    DateOnly SlotDate,
    int Capacity
) : IRequest<CreatePoolSlotResponse>;

// Handler 
public class CreateSlotCommandHandler : IRequestHandler<CreateSlotCommand, CreatePoolSlotResponse>
{
    private readonly IUnitOfWork _uow;

    public CreateSlotCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<CreatePoolSlotResponse> Handle(CreateSlotCommand request, CancellationToken ct)
    {
        // 1. Kiểm tra pool tồn tại
        var pool = await _uow.FirstOrDefaultAsync(
            _uow.Repository<Pool>().Query().Where(p => p.PoolId == request.PoolId), ct)
            ?? throw new NotFoundException(nameof(Pool), request.PoolId);

        // 2. Slot phải nằm trong giờ mở cửa của pool
        if (request.StartTime < pool.OpeningTime || request.EndTime > pool.ClosingTime)
            throw new BadRequestException(
                $"Slot phải nằm trong giờ mở cửa của bể bơi ({pool.OpeningTime:hh\\:mm} – {pool.ClosingTime:hh\\:mm}).");

        if (request.Capacity < 1 || request.Capacity > pool.StandardCapacity)
            throw new BadRequestException($"Sức chứa ca bơi phải lớn hơn 0 và không vượt quá giới hạn an toàn của bể bơi ({pool.StandardCapacity} người).");

        // 3. Kiểm tra trùng slot cùng pool, cùng ngày (overlap)
        bool hasOverlap = await _uow.AnyAsync(
            _uow.Repository<PoolSlot>().Query()
                .Where(s => s.PoolId    == request.PoolId
                         && s.SlotDate  == request.SlotDate
                         && s.StartTime <  request.EndTime
                         && s.EndTime   >  request.StartTime), ct);

        if (hasOverlap)
            throw new BadRequestException("Đã tồn tại slot trùng khung giờ trong bể bơi này.");

        var slot = new PoolSlot
        {
            PoolId    = request.PoolId,
            SlotName  = request.SlotName,
            StartTime = request.StartTime,
            EndTime   = request.EndTime,
            SlotDate  = request.SlotDate,
            Capacity  = request.Capacity,
            Status    = "Open",
            CreatedAt = DateTime.UtcNow
        };

        await _uow.Repository<PoolSlot>().AddAsync(slot, ct);
        await _uow.SaveChangesAsync(ct);

        // Auto-generate SlotName nếu không truyền
        string displayName = slot.SlotName
            ?? $"{request.StartTime:hh\\:mm} - {request.EndTime:hh\\:mm}";

        return new CreatePoolSlotResponse
        {
            PoolSlotId = slot.PoolSlotId,
            PoolId     = slot.PoolId,
            SlotName   = displayName,
            StartTime  = slot.StartTime.ToString(@"hh\:mm"),
            EndTime    = slot.EndTime.ToString(@"hh\:mm"),
            SlotDate   = slot.SlotDate.ToString("yyyy-MM-dd"),
            Capacity   = slot.Capacity,
            Status     = slot.Status
        };
    }
}

// ── Validator ─────────────────────────────────────────────────────────────────
public class CreateSlotCommandValidator : AbstractValidator<CreateSlotCommand>
{
    public CreateSlotCommandValidator() { 
        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime)
            .WithMessage("Giờ kết thúc phải lớn hơn giờ bắt đầu.");

        RuleFor(x => x.SlotDate) 
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Ngày slot không được là ngày trong quá khứ.");
    }
}
