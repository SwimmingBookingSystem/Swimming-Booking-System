using System;

namespace SBS.Domain.Entities;

public class PoolStaffAssignment
{
    public int AssignmentId { get; set; }

    public int PoolId { get; set; }

    public Guid StaffId { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    public Guid? AssignedByAdminId { get; set; }

    // Navigation properties
    public virtual Pool Pool { get; set; } = null!;
}
