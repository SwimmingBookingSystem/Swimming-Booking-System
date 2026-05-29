using MediatR;
using Microsoft.EntityFrameworkCore;
using SBS.Application.Common.Interfaces;
using SBS.Application.Common.Models;
using SBS.Application.Features.ServiceStaff.DTOs;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.ServiceStaff.Queries;

// ── QUERY ─────────────────────────────────────────────────────────────────────

public record GetServiceReportsQuery(
    string? Status,
    int Page,
    int PageSize
) : IRequest<PagedResult<ServiceReportListItemDto>>;

// ── HANDLER ───────────────────────────────────────────────────────────────────

public sealed class GetServiceReportsQueryHandler 
    : IRequestHandler<GetServiceReportsQuery, PagedResult<ServiceReportListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetServiceReportsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ServiceReportListItemDto>> Handle(
        GetServiceReportsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ServiceReports
            .Include(sr => sr.PoolService)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var statusLower = request.Status.ToLower();
            query = query.Where(sr => sr.Status.ToLower() == statusLower);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var reports = await query
            .OrderByDescending(sr => sr.ReportDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(sr => new ServiceReportListItemDto
            {
                ReportId = sr.ReportId,
                ServiceName = sr.PoolService.ServiceName,
                ReportReason = sr.ReportReason,
                ReportDate = sr.ReportDate,
                Status = sr.Status,
                ManagerNote = sr.ManagerNote,
                ProcessedAt = sr.ProcessedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<ServiceReportListItemDto>
        {
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            Items = reports
        };
    }
}
