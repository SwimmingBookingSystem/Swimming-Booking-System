using MediatR;
using Microsoft.EntityFrameworkCore;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.ServiceStaff.DTOs;
using SBS.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.ServiceStaff.Commands;

// ── COMMAND ───────────────────────────────────────────────────────────────────

public record CreateServiceReportCommand(CreateServiceReportRequestDto Request) : IRequest<ServiceReportResponseDto>;

// ── HANDLER ───────────────────────────────────────────────────────────────────

public sealed class CreateServiceReportCommandHandler : IRequestHandler<CreateServiceReportCommand, ServiceReportResponseDto>
{
    private readonly IApplicationDbContext _context;

    public CreateServiceReportCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceReportResponseDto> Handle(CreateServiceReportCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        // 1. Verify PoolService exists
        var poolService = await _context.PoolServices
            .FirstOrDefaultAsync(ps => ps.PoolServiceId == request.ServiceId, cancellationToken)
            ?? throw new InvalidOperationException($"Không tìm thấy dịch vụ với ID {request.ServiceId}.");

        // 2. Verify Staff exists
        var staffExists = await _context.Staffs
            .AnyAsync(s => s.StaffId == request.StaffId, cancellationToken);
        if (!staffExists)
            throw new InvalidOperationException($"Không tìm thấy nhân viên với ID {request.StaffId}.");

        // 3. Create ServiceReport
        var report = new ServiceReport
        {
            StaffId = request.StaffId,
            ServiceId = request.ServiceId,
            ReportReason = request.ReportReason,
            Suggestion = request.Suggestion,
            ReportDate = DateTime.UtcNow,
            Status = "pending" // Initial status when created
        };

        _context.ServiceReports.Add(report);
        await _context.SaveChangesAsync(cancellationToken);

        return new ServiceReportResponseDto
        {
            ReportId = report.ReportId,
            StaffId = report.StaffId,
            ServiceId = report.ServiceId,
            ServiceName = poolService.ServiceName,
            ReportReason = report.ReportReason,
            Suggestion = report.Suggestion,
            ReportDate = report.ReportDate,
            Status = report.Status,
            Message = "Báo cáo sự cố dịch vụ đã được gửi thành công đến Quản lý."
        };
    }
}
