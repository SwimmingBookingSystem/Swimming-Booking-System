using System;

namespace SBS.Domain.Entities;

public class WaitlistEntry
{
    public int WaitlistEntryId { get; set; }
    public Guid UserId { get; set; } // FK → AppUser
    public int PoolSlotId { get; set; }
    public int Quantity { get; set; } = 1;
    public int Position { get; set; }
    public string Status { get; set; } = "Waiting";
    public DateTime? NotifiedAt { get; set; }
    public DateTime? Deadline { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties (domain entities only, no AppUser)
    public virtual PoolSlot PoolSlot { get; set; } = null!;
}
