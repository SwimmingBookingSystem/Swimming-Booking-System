namespace SBS.Application.Features.Manager.Slots.Dtos;

public class CreatePoolSlotResponse
{
    public int PoolSlotId { get; set; }
    public int PoolId { get; set; }
    public string? SlotName { get; set; }
    public string StartTime { get; set; } = null!;
    public string EndTime { get; set; } = null!;
    public string SlotDate { get; set; } = null!;
    public int Capacity { get; set; }
    public string Status { get; set; } = null!;
}
