using FluentValidation;
using MediatR;
using SBS.Application.Common.Dtos.Manager;
using SBS.Application.Common.Interfaces;
using SBS.Application.Common.ManagerExceptions;
using SBS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Manager.Slots.Commands.GenerateSlots;

public record GenerateSlotsCommand(
    int PoolId,
    DateOnly StartDate,
    DateOnly EndDate,
    int DurationMinutes,
    int BreakMinutes
) : IRequest<SuccessResponse>;

public class GenerateSlotsCommandHandler : IRequestHandler<GenerateSlotsCommand, SuccessResponse>
{
    private readonly SBS.Application.Features.Manager.Services.Interfaces.ISlotManagementService _slotService;

    public GenerateSlotsCommandHandler(SBS.Application.Features.Manager.Services.Interfaces.ISlotManagementService slotService) => _slotService = slotService;

    public async Task<SuccessResponse> Handle(GenerateSlotsCommand request, CancellationToken ct)
    {
        return await _slotService.GenerateSlotsAsync(request, ct);
    }
}

public class GenerateSlotsCommandValidator : AbstractValidator<GenerateSlotsCommand>
{
    public GenerateSlotsCommandValidator()
    {
        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0).WithMessage("Thời lượng ca bơi phải lớn hơn 0.");
            
        RuleFor(x => x.BreakMinutes)
            .GreaterThanOrEqualTo(0).WithMessage("Thời gian nghỉ không được âm.");
            
        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.");
            
        RuleFor(x => x.StartDate)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow)).WithMessage("Ngày bắt đầu không được trong quá khứ.");
    }
}

