using MediatR;
using SBS.Application.Common.Dtos.Manager;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.Manager.Slots.Dtos;
using SBS.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Manager.Slots.Queries.GetSlotsByPool;

public record GetSlotsByPoolQuery(
    int PoolId,
    int Page = 1,
    int PageSize = 10,
    string? Date = null,    // "yyyy-MM-dd"
    string? Status = null
) : IRequest<PagedResponse<PoolSlotDto>>;

public class GetSlotsByPoolQueryHandler : IRequestHandler<GetSlotsByPoolQuery, PagedResponse<PoolSlotDto>>
{
    private readonly IReadOnlyUnitOfWork _uow;

    public GetSlotsByPoolQueryHandler(IReadOnlyUnitOfWork uow) => _uow = uow;

    public async Task<PagedResponse<PoolSlotDto>> Handle(GetSlotsByPoolQuery request, CancellationToken ct)
    {
        var query = _uow.Repository<PoolSlot>().Query()
                        .Where(s => s.PoolId == request.PoolId);

        if (!string.IsNullOrWhiteSpace(request.Status))
            query = query.Where(s => s.Status == request.Status);

        if (!string.IsNullOrWhiteSpace(request.Date) &&
            DateOnly.TryParse(request.Date, out var date))
            query = query.Where(s => s.SlotDate == date);

        var total = await _uow.CountAsync(query, ct);

        var items = await _uow.ToListAsync(
            query.OrderBy(s => s.SlotDate)
                 .ThenBy(s => s.StartTime)
                 .Skip((request.Page - 1) * request.PageSize)
                 .Take(request.PageSize)
                 .Select(s => new PoolSlotDto
                 {
                     PoolSlotId = s.PoolSlotId,
                     PoolId     = s.PoolId,
                     SlotName   = s.SlotName,
                     StartTime  = s.StartTime.ToString(@"hh\:mm"),
                     EndTime    = s.EndTime.ToString(@"hh\:mm"),
                     SlotDate   = s.SlotDate.ToString("yyyy-MM-dd"),
                     Capacity   = s.Capacity,
                     AvailableCapacity = s.Capacity - s.Bookings.Where(b => b.Status != "Cancelled" && b.Status != "Failed").SelectMany(b => b.BookingDetails).Sum(bd => bd.Quantity),
                     Status     = s.Status,
                     CreatedAt  = s.CreatedAt
                 }), ct);

        return new PagedResponse<PoolSlotDto>
        {
            Items      = items,
            TotalCount = total,
            Page       = request.Page,
            PageSize   = request.PageSize
        };
    }
}
