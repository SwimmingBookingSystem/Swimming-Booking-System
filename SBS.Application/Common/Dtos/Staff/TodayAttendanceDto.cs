using System;
using System.Collections.Generic;

namespace SBS.Application.Common.Dtos.Staff;

public record TodayAttendanceDto
{
    public int TotalBookings { get; init; }
    public int CheckedIn { get; init; }
    public int NotCheckedIn { get; init; }
    public List<TodayGuestItemDto> Guests { get; init; } = new();
}

public record TodayGuestItemDto
{
    public int BookingId { get; init; }
    public string BookingCode { get; init; } = null!;
    public string CustomerName { get; init; } = null!;
    public string SlotTime { get; init; } = null!;
    public bool IsCheckedIn { get; init; }
    public DateTime? CheckInTime { get; init; }
}
