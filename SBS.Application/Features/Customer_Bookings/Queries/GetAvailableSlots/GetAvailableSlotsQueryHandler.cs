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

namespace SBS.Application.Features.Customer_Bookings.Queries.GetAvailableSlots;

public class GetAvailableSlotsQueryHandler : IRequestHandler<GetAvailableSlotsQuery, List<AvailableSlotDto>>
{
    private readonly IReadOnlyUnitOfWork _readOnlyUnitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetAvailableSlotsQueryHandler(IReadOnlyUnitOfWork readOnlyUnitOfWork, ICurrentUserService currentUserService)
    {
        _readOnlyUnitOfWork = readOnlyUnitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<List<AvailableSlotDto>> Handle(GetAvailableSlotsQuery request, CancellationToken cancellationToken)
    {
        var userIdStr = _currentUserService.UserId;
        var (today, timeNow) = BookingTimePolicy.GetVietnamDateAndTime(DateTime.UtcNow);

        Guid? currentUserId = !string.IsNullOrEmpty(userIdStr) && Guid.TryParse(userIdStr, out var id) ? id : null;

        // Fetch the raw slots with related entities
        var rawSlots = await _readOnlyUnitOfWork.Repository<PoolSlot>().Query()
            .AsNoTracking()
            .Include(s => s.Pool)
            .Include(s => s.WaitlistEntries.Where(w => w.Status == "Waiting"))
            .Include(s => s.Bookings.Where(b => b.Status != "Cancelled" && b.Status != "Failed" && b.Status != "Refunded"))
                .ThenInclude(b => b.BookingDetails)
            .Where(s => s.PoolId == request.PoolId && s.SlotDate == request.Date && s.Capacity > 0 && s.Status == "Open")
            .OrderBy(s => s.StartTime)
            .ToListAsync(cancellationToken);

        // Map to DTO in memory to allow complex waitlist position calculation
        var slots = rawSlots.Select(s => 
        {
            var currentUserWaitlistEntry = currentUserId.HasValue 
                ? s.WaitlistEntries.FirstOrDefault(w => w.UserId == currentUserId.Value) 
                : null;

            int? waitlistPosition = null;
            if (currentUserWaitlistEntry != null)
            {
                // Calculate position relative to others waiting
                waitlistPosition = s.WaitlistEntries
                    .Count(w => w.Position <= currentUserWaitlistEntry.Position);
            }

            return new AvailableSlotDto
            {
                PoolSlotId = s.PoolSlotId,
                PoolId = s.PoolId,
                PoolName = s.Pool.PoolName,
                SlotName = s.SlotName,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                SlotDate = s.SlotDate,
                Capacity = s.Capacity,
                AvailableCapacity = s.Capacity - (s.Bookings.SelectMany(b => b.BookingDetails).Sum(bd => (int?)bd.Quantity) ?? 0),
                
                // Waitlist specific fields
                IsBookingClosed = BookingTimePolicy.IsBookingClosed(s.SlotDate, s.EndTime, today, timeNow),
                TotalWaitlistCount = s.WaitlistEntries.Count,
                IsInWaitlist = currentUserWaitlistEntry != null,
                WaitlistPosition = waitlistPosition,
                WaitlistEntryId = currentUserWaitlistEntry?.WaitlistEntryId
            };
        }).ToList();

        return slots;
    }
}
