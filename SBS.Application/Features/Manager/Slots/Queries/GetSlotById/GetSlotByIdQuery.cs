using MediatR;
using SBS.Application.Common.ManagerExceptions;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.Manager.Slots.Dtos;
using SBS.Domain.Entities;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Manager.Slots.Queries.GetSlotById;

public record GetSlotByIdQuery(int SlotId) : IRequest<PoolSlotDto>;

public class GetSlotByIdQueryHandler : IRequestHandler<GetSlotByIdQuery, PoolSlotDto>
{
    private readonly IUnitOfWork _uow;

    public GetSlotByIdQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<PoolSlotDto> Handle(GetSlotByIdQuery request, CancellationToken ct)
    {
        var slot = await _uow.FirstOrDefaultAsync(
            _uow.Repository<PoolSlot>().Query()
                .Where(s => s.PoolSlotId == request.SlotId), ct)
            ?? throw new NotFoundException(nameof(PoolSlot), request.SlotId);

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
