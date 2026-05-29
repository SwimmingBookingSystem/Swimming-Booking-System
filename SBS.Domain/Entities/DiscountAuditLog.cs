using System;

namespace SBS.Domain.Entities;

public class DiscountAuditLog
{
    public int LogId { get; set; }
    public int DiscountId { get; set; }
    public Guid ManagerId { get; set; }
    public string ActionType { get; set; } = null!;
    public DateTime ActionTime { get; set; } = DateTime.UtcNow;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Discount Discount { get; set; } = null!;
}
