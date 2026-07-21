using FluentValidation;
using MediatR;
using SBS.Application.Common.ManagerExceptions;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.Manager.Slots.Dtos;
using SBS.Domain.Entities;
using System;
using Microsoft.EntityFrameworkCore;
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
    private readonly SBS.Application.Features.Manager.Services.Interfaces.ISlotManagementService _slotService;

    public UpdateSlotCommandHandler(SBS.Application.Features.Manager.Services.Interfaces.ISlotManagementService slotService) => _slotService = slotService;

    public async Task<PoolSlotDto> Handle(UpdateSlotCommand request, CancellationToken ct)
    {
        return await _slotService.UpdateSlotAsync(request, ct);
    }
}

// ── Validator ─────────────────────────────────────────────────────────────────
public class UpdateSlotCommandValidator : AbstractValidator<UpdateSlotCommand>
{
    public UpdateSlotCommandValidator() { 
        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime)
            .WithMessage("Giờ kết thúc phải lớn hơn giờ bắt đầu.");
    }
}

