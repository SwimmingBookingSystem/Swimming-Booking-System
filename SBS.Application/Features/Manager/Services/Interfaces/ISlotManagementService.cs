using SBS.Application.Common.Dtos.Manager;
using SBS.Application.Features.Manager.Slots.Commands.CreateSlot;
using SBS.Application.Features.Manager.Slots.Commands.GenerateSlots;
using SBS.Application.Features.Manager.Slots.Commands.UpdateSlot;
using SBS.Application.Features.Manager.Slots.Dtos;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Manager.Services.Interfaces;

public interface ISlotManagementService
{
    Task<SuccessResponse> CloseSlotAsync(int slotId, CancellationToken ct);
    Task<PoolSlotDto> CreateSlotAsync(CreateSlotCommand request, CancellationToken ct);
    Task<SuccessResponse> GenerateSlotsAsync(GenerateSlotsCommand request, CancellationToken ct);
    Task<SuccessResponse> OpenSlotAsync(int slotId, CancellationToken ct);
    Task<PoolSlotDto> UpdateSlotAsync(UpdateSlotCommand request, CancellationToken ct);
    Task<SuccessResponse> UpdateSlotCapacityAsync(int slotId, int capacity, CancellationToken ct);
}
