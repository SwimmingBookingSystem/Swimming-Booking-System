using System;

namespace SBS.Domain.Entities;

public class DeviceReport
{
    public int ReportId { get; set; }
    public int StaffId { get; set; }
    public int? DeviceId { get; set; }
    public string ReportReason { get; set; } = null!;
    public string? Suggestion { get; set; }
    public DateTime ReportDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = null!;
    public string? ManagerNote { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public Guid? ProcessedBy { get; set; }

    // Navigation properties
    public virtual Staff Staff { get; set; } = null!;
    public virtual PoolDevice? PoolDevice { get; set; }
}
