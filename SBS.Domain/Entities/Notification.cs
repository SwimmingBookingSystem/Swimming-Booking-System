using System;

namespace SBS.Domain.Entities;

public class Notification
{
    public int NotificationId { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? TargetRoleId { get; set; }
    public int? TargetBranchId { get; set; }

    // Navigation properties
    public virtual Branch? Branch { get; set; }
}
