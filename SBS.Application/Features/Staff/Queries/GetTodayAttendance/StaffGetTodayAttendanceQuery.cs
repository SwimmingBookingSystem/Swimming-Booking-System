using MediatR;
using Microsoft.EntityFrameworkCore;
using SBS.Application.Common.Dtos.Staff;
using SBS.Application.Common.Interfaces;
using SBS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Staff.Queries.GetTodayAttendance;

public record StaffGetTodayAttendanceQuery : IRequest<TodayAttendanceDto>
{
    /// <summary>Lọc theo bể bơi cụ thể. Null = tất cả bể.</summary>
    public int? PoolId { get; init; }
}

public class StaffGetTodayAttendanceQueryHandler : IRequestHandler<StaffGetTodayAttendanceQuery, TodayAttendanceDto>
{
    private readonly IReadOnlyUnitOfWork _readOnlyUnitOfWork;
    private readonly IStaffUserService _staffUserService;

    public StaffGetTodayAttendanceQueryHandler(
        IReadOnlyUnitOfWork readOnlyUnitOfWork,
        IStaffUserService staffUserService)
    {
        _readOnlyUnitOfWork = readOnlyUnitOfWork;
        _staffUserService = staffUserService;
    }

    public async Task<TodayAttendanceDto> Handle(StaffGetTodayAttendanceQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7)); // UTC+7

        var query = _readOnlyUnitOfWork.Repository<Booking>()
            .Query()
            .Include(b => b.PoolSlot)
                .ThenInclude(s => s.Pool)
            .Include(b => b.CheckIn)
            .Where(b => b.BookingDate == today)
            .Where(b => b.Status == "Confirmed" || b.Status == "CheckIn" || b.Status == "NoShow")
            .AsQueryable();

        if (request.PoolId.HasValue)
            query = query.Where(b => b.PoolSlot.PoolId == request.PoolId.Value);

        var bookings = await _readOnlyUnitOfWork.ToListAsync(query, cancellationToken);

        // Enrich thông tin khách hàng từ IStaffUserService
        var guests = new List<TodayGuestItemDto>(bookings.Count);
        foreach (var b in bookings)
        {
            var customer = await _staffUserService.GetUserBriefAsync(b.UserId, cancellationToken);
            guests.Add(new TodayGuestItemDto
            {
                BookingId = b.BookingId,
                BookingCode = b.BookingCode,
                CustomerName = customer?.FullName ?? "Khách vãng lai",
                SlotTime = $"{b.PoolSlot.StartTime:hh\\:mm} - {b.PoolSlot.EndTime:hh\\:mm}",
                IsCheckedIn = b.CheckIn is not null,
                CheckInTime = b.CheckIn?.CheckInTime
            });
        }

        var checkedInCount = guests.Count(g => g.IsCheckedIn);

        return new TodayAttendanceDto
        {
            TotalBookings = guests.Count,
            CheckedIn = checkedInCount,
            NotCheckedIn = guests.Count - checkedInCount,
            Guests = guests.OrderBy(g => g.SlotTime).ThenBy(g => g.IsCheckedIn).ToList()
        };
    }
}
