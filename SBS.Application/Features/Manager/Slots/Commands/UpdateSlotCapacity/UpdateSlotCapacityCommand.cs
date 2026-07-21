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
    private readonly SBS.Application.Features.Manager.Services.Interfaces.ISlotManagementService _slotService;

    public UpdateSlotCapacityCommandHandler(SBS.Application.Features.Manager.Services.Interfaces.ISlotManagementService slotService) => _slotService = slotService;

    public async Task<SuccessResponse> Handle(UpdateSlotCapacityCommand request, CancellationToken ct)
    {
        return await _slotService.UpdateSlotCapacityAsync(request.SlotId, request.Capacity, ct);
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

