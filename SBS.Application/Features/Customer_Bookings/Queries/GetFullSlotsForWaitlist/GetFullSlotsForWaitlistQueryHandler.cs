using MediatR;
using Microsoft.EntityFrameworkCore;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.Customer_Bookings.Dtos;
using SBS.Application.Features.Customer_Bookings.Policies;
using SBS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Customer_Bookings.Queries.GetFullSlotsForWaitlist;

public class GetFullSlotsForWaitlistQueryHandler : IRequestHandler<GetFullSlotsForWaitlistQuery, List<AvailableSlotDto>>
{
    private readonly IReadOnlyUnitOfWork _readOnlyUnitOfWork;

    public GetFullSlotsForWaitlistQueryHandler(IReadOnlyUnitOfWork readOnlyUnitOfWork)
    {
        _readOnlyUnitOfWork = readOnlyUnitOfWork;
    }

    public async Task<List<AvailableSlotDto>> Handle(GetFullSlotsForWaitlistQuery request, CancellationToken cancellationToken)
    {
        var (today, timeNow) = BookingTimePolicy.GetVietnamDateAndTime(DateTime.UtcNow);

        var slots = await _readOnlyUnitOfWork.Repository<PoolSlot>().Query()
            .AsNoTracking()
            .Include(s => s.Pool)
            .Where(s => s.PoolId == request.PoolId && s.Capacity > 0 && s.Status == "Open")
            .Where(s => s.SlotDate >= today)
            .OrderBy(s => s.SlotDate)
            .ThenBy(s => s.StartTime)
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
                AvailableCapacity = s.Capacity - (s.Bookings
                    .Where(b => b.Status != "Cancelled" && b.Status != "Failed" && b.Status != "Refunded")
                    .SelectMany(b => b.BookingDetails)
                    .Sum(bd => (int?)bd.Quantity) ?? 0)
            })
            .ToListAsync(cancellationToken);

        // Lọc những slot có AvailableCapacity <= 0 (đã full)
        foreach (var slot in slots)
        {
            slot.IsBookingClosed = BookingTimePolicy.IsBookingClosed(
                slot.SlotDate, slot.EndTime, today, timeNow);
        }

        var fullSlots = slots
            .Where(s => !s.IsBookingClosed && s.AvailableCapacity <= 0)
            .ToList();

        return fullSlots;
    }
}
