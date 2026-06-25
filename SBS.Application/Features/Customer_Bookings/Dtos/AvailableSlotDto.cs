using System;

namespace SBS.Application.Features.Customer_Bookings.Dtos;

public class AvailableSlotDto
{
    public int PoolSlotId { get; set; }
    public int PoolId { get; set; }
    public string PoolName { get; set; } = null!;
    public string? SlotName { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public DateOnly SlotDate { get; set; }
    public int Capacity { get; set; }
}
