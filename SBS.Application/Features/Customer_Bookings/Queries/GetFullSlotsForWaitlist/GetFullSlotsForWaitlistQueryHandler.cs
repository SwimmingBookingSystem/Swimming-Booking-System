using MediatR;
using Microsoft.EntityFrameworkCore;
using SBS.Application.Common;
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
            .Include(s => s.Bookings.Where(b =>
                b.Status == BookingStatus.PendingPayment ||
                b.Status == BookingStatus.Paid ||
                b.Status == BookingStatus.CheckIn))
                .ThenInclude(b => b.BookingDetails)
                    .ThenInclude(bd => bd.PoolTicketType)
                        .ThenInclude(pt => pt.TicketType)
                            .ThenInclude(tt => tt.ComboItems)
            .Where(s => s.PoolId == request.PoolId && s.Capacity > 0 && s.Status == "Open" && s.SlotDate >= today)
            .OrderBy(s => s.SlotDate)
            .ThenBy(s => s.StartTime)
            .ToListAsync(cancellationToken);

        return slots
            .Select(slot => new AvailableSlotDto
            {
                PoolSlotId = slot.PoolSlotId,
                PoolId = slot.PoolId,
                PoolName = slot.Pool.PoolName,
                SlotName = slot.SlotName,
                StartTime = slot.StartTime,
                EndTime = slot.EndTime,
                SlotDate = slot.SlotDate,
                Capacity = slot.Capacity,
                AvailableCapacity = Math.Max(0, slot.Capacity - CalculateBookedCapacity(slot)),
                IsBookingClosed = BookingTimePolicy.IsBookingClosed(slot.SlotDate, slot.EndTime, today, timeNow)
            })
            .Where(slot => !slot.IsBookingClosed && slot.AvailableCapacity <= 0)
            .ToList();
    }

    private static int CalculateBookedCapacity(PoolSlot slot) => slot.Bookings
        .SelectMany(booking => booking.BookingDetails)
        .Sum(detail =>
        {
            var ticketType = detail.PoolTicketType.TicketType;
            var slotEquivalent = string.Equals(ticketType.Category, "Combo", StringComparison.OrdinalIgnoreCase)
                ? ticketType.ComboItems.Sum(item => item.Quantity)
                : 1;

            return detail.Quantity * Math.Max(1, slotEquivalent);
        });
}