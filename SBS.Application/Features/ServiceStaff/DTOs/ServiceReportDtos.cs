using System;

namespace SBS.Application.Features.ServiceStaff.DTOs;

// ── REQUEST ──────────────────────────────────────────────────────────────────

public class CreateServiceReportRequestDto
{
    public int ServiceId { get; set; }
    public string ReportReason { get; set; } = null!;
    public string? Suggestion { get; set; }
    
    /// <summary>
    /// StaffId of the staff member performing the report.
    /// TODO: Replace with JWT claim when auth is configured by auth team.
    /// </summary>
    public int StaffId { get; set; }
}

// ── RESPONSE (CREATED) ────────────────────────────────────────────────────────

public class ServiceReportResponseDto
{
    public int ReportId { get; set; }
    public int StaffId { get; set; }
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = null!;
    public string ReportReason { get; set; } = null!;
    public string? Suggestion { get; set; }
    public DateTime ReportDate { get; set; }
    public string Status { get; set; } = null!;
    public string Message { get; set; } = null!;
}

// ── LIST ITEM ─────────────────────────────────────────────────────────────────

public class ServiceReportListItemDto
{
    public int ReportId { get; set; }
    public string ServiceName { get; set; } = null!;
    public string ReportReason { get; set; } = null!;
    public DateTime ReportDate { get; set; }
    public string Status { get; set; } = null!;
    public string? ManagerNote { get; set; }
    public DateTime? ProcessedAt { get; set; }
}

// ── DETAIL ────────────────────────────────────────────────────────────────────

public class ServiceReportDetailDto
{
    public int ReportId { get; set; }
    public int StaffId { get; set; }
    public string? StaffName { get; set; }
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = null!;
    public string ReportReason { get; set; } = null!;
    public string? Suggestion { get; set; }
    public DateTime ReportDate { get; set; }
    public string Status { get; set; } = null!;
    public string? ManagerNote { get; set; }
    public int? ProcessedBy { get; set; }
    public DateTime? ProcessedAt { get; set; }
}
