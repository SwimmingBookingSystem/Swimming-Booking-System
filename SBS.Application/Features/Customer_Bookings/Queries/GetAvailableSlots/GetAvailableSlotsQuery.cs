using MediatR;
using Microsoft.EntityFrameworkCore;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.Customer_Bookings.Dtos;
using SBS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Customer_Bookings.Queries.GetAvailableSlots;

public record GetAvailableSlotsQuery(int PoolId, DateOnly Date) : IRequest<List<AvailableSlotDto>>;

public class GetAvailableSlotsQueryHandler : IRequestHandler<GetAvailableSlotsQuery, List<AvailableSlotDto>>
{
    private readonly IReadOnlyUnitOfWork _readOnlyUnitOfWork;

    public GetAvailableSlotsQueryHandler(IReadOnlyUnitOfWork readOnlyUnitOfWork)
    {
        _readOnlyUnitOfWork = readOnlyUnitOfWork;
    }

    public async Task<List<AvailableSlotDto>> Handle(GetAvailableSlotsQuery request, CancellationToken cancellationToken)
    {
        // Use read-only unit of work for best performance
        var slots = await _readOnlyUnitOfWork.Repository<PoolSlot>().Query()
            .AsNoTracking()
            .Include(s => s.Pool)
            .Where(s => s.PoolId == request.PoolId && s.SlotDate == request.Date && s.Capacity > 0 && s.Status == "Open")
            .OrderBy(s => s.StartTime)
            .Select(s => new AvailableSlotDto
            {
                PoolSlotId = s.PoolSlotId,
                PoolId = s.PoolId,
                PoolName = s.Pool.PoolName,
                SlotName = s.SlotName,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                SlotDate = s.SlotDate,
                Capacity = s.Capacity,
                AvailableCapacity = s.Capacity - s.Bookings
                    .Where(b => b.Status != "Cancelled" && b.Status != "Failed" && b.Status != "Refunded")
                    .SelectMany(b => b.BookingDetails)
                    .Sum(bd => (int?)bd.Quantity) ?? s.Capacity
            })
            .ToListAsync(cancellationToken);

        return slots;
    }
}
