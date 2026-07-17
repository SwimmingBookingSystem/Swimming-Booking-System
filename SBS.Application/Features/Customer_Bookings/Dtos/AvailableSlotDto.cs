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
    public int AvailableCapacity { get; set; }
    
    // Waitlist tracking
    public bool IsInWaitlist { get; set; }
    public int? WaitlistPosition { get; set; }
    public int TotalWaitlistCount { get; set; }
    public int? WaitlistEntryId { get; set; }
}
