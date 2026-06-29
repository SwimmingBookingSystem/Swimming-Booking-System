using System;

namespace SBS.Application.Common.Dtos.Admin;

public record StaffAssignmentDto
{
    public int AssignmentId { get; init; }
    public int PoolId { get; init; }
    public string PoolName { get; init; } = null!;
    public Guid StaffId { get; init; }
    public string StaffName { get; init; } = null!;
    public string StaffEmail { get; init; } = null!;
    public DateTime AssignedAt { get; init; }
}
