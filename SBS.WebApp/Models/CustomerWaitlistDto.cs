using System;

namespace SBS.WebApp.Models;

/// <summary>
/// Local DTO mirroring the API response for customer waitlist entries.
/// WebApp does NOT reference SBS.Application, so DTOs used for JSON deserialization
/// are duplicated here with the same shape.
/// </summary>
public class CustomerWaitlistDto
{
    public int WaitlistEntryId { get; set; }
    public int PoolId { get; set; }
    public string PoolName { get; set; } = null!;
    public DateOnly SlotDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int Quantity { get; set; }
    public string Status { get; set; } = null!;
    public int CurrentPosition { get; set; }
    public int TotalWaitlistCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
