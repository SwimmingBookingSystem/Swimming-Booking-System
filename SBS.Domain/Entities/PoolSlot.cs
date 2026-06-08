using System;
using System.Collections.Generic;

namespace SBS.Domain.Entities;

public class PoolSlot
{
    public int PoolSlotId { get; set; }
    public int PoolId { get; set; }
    public string? SlotName { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public DateOnly SlotDate { get; set; }
    public int Capacity { get; set; }
    public string Status { get; set; } = "Open";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Pool Pool { get; set; } = null!;
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public virtual ICollection<WaitlistEntry> WaitlistEntries { get; set; } = new List<WaitlistEntry>();
}
