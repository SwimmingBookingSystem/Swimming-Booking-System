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
    private readonly SBS.Application.Features.Manager.Services.Interfaces.ISlotManagementService _slotService;

    public CreateSlotCommandHandler(SBS.Application.Features.Manager.Services.Interfaces.ISlotManagementService slotService) => _slotService = slotService;

    public async Task<CreatePoolSlotResponse> Handle(CreateSlotCommand request, CancellationToken ct)
    {
        var dto = await _slotService.CreateSlotAsync(request, ct);
        return new CreatePoolSlotResponse
        {
            PoolSlotId = dto.PoolSlotId,
            PoolId     = dto.PoolId,
            SlotName   = dto.SlotName,
            StartTime  = dto.StartTime,
            EndTime    = dto.EndTime,
            SlotDate   = dto.SlotDate,
            Capacity   = dto.Capacity,
            Status     = dto.Status
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

