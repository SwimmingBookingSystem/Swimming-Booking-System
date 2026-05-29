using MediatR;
using Microsoft.EntityFrameworkCore;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.ServiceStaff.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.ServiceStaff.Queries;

// ── QUERY ─────────────────────────────────────────────────────────────────────

public record GetServiceReportDetailQuery(int ReportId) : IRequest<ServiceReportDetailDto>;

// ── HANDLER ───────────────────────────────────────────────────────────────────

public sealed class GetServiceReportDetailQueryHandler : IRequestHandler<GetServiceReportDetailQuery, ServiceReportDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public GetServiceReportDetailQueryHandler(IApplicationDbContext context, IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task<ServiceReportDetailDto> Handle(GetServiceReportDetailQuery request, CancellationToken cancellationToken)
    {
        var report = await _context.ServiceReports
            .Include(sr => sr.PoolService)
            .Include(sr => sr.Staff)
            .AsNoTracking()
            .FirstOrDefaultAsync(sr => sr.ReportId == request.ReportId, cancellationToken)
            ?? throw new KeyNotFoundException($"Không tìm thấy báo cáo sự cố với ID {request.ReportId}.");

        // Resolve staff name using Staff.UserId
        string? staffName = null;
        if (report.Staff != null)
        {
            staffName = await _identityService.GetUserFullNameAsync(report.Staff.UserId, cancellationToken);
        }

        return new ServiceReportDetailDto
        {
            ReportId = report.ReportId,
            StaffId = report.StaffId,
            StaffName = staffName ?? "Nhân viên",
            ServiceId = report.ServiceId,
            ServiceName = report.PoolService.ServiceName,
            ReportReason = report.ReportReason,
            Suggestion = report.Suggestion,
            ReportDate = report.ReportDate,
            Status = report.Status,
            ManagerNote = report.ManagerNote,
            ProcessedBy = report.ProcessedBy,
            ProcessedAt = report.ProcessedAt
        };
    }
}
