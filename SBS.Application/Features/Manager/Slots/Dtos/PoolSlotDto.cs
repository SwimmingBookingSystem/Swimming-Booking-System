using System;

namespace SBS.Application.Features.Manager.Slots.Dtos;

public class PoolSlotDto
{
    public int PoolSlotId { get; set; }
    public int PoolId { get; set; }
    public string? SlotName { get; set; }
    public string StartTime { get; set; } = null!;  // "HH:mm"
    public string EndTime { get; set; } = null!;    // "HH:mm"
    public string SlotDate { get; set; } = null!;   // "yyyy-MM-dd"
    public int Capacity { get; set; }
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
