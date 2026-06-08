using System;

namespace SBS.Application.Features.Manager.Pools.Dtos;

public class PoolDto
{
    public int PoolId { get; set; }
    public string PoolName { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string OpeningTime { get; set; } = null!;  // "HH:mm"
    public string ClosingTime { get; set; } = null!;  // "HH:mm"
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
