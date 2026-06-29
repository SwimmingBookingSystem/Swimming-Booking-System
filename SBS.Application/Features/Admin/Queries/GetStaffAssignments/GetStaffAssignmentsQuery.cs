using MediatR;
using Microsoft.EntityFrameworkCore;
using SBS.Application.Common.Dtos.Admin;
using SBS.Application.Common.Interfaces;
using SBS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Admin.Queries.GetStaffAssignments;

public record GetStaffAssignmentsQuery : IRequest<List<StaffAssignmentDto>>
{
    public Guid? StaffId { get; init; }

    public int? PoolId { get; init; }
}

public class GetStaffAssignmentsQueryHandler : IRequestHandler<GetStaffAssignmentsQuery, List<StaffAssignmentDto>>
{
    private readonly IReadOnlyUnitOfWork _readOnlyUnitOfWork;
    private readonly IStaffUserService _staffUserService;

    public GetStaffAssignmentsQueryHandler(
        IReadOnlyUnitOfWork readOnlyUnitOfWork,
        IStaffUserService staffUserService)
    {
        _readOnlyUnitOfWork = readOnlyUnitOfWork;
        _staffUserService = staffUserService;
    }

    public async Task<List<StaffAssignmentDto>> Handle(GetStaffAssignmentsQuery request, CancellationToken cancellationToken)
    {
        var query = _readOnlyUnitOfWork.Repository<PoolStaffAssignment>()
            .Query()
            .Include(a => a.Pool)
            .AsQueryable();

        if (request.StaffId.HasValue)
            query = query.Where(a => a.StaffId == request.StaffId.Value);

        if (request.PoolId.HasValue)
            query = query.Where(a => a.PoolId == request.PoolId.Value);

        var assignments = await _readOnlyUnitOfWork.ToListAsync(
            query.OrderBy(a => a.Pool.PoolName),
            cancellationToken);

        // Enrich thông tin Staff từ IStaffUserService
        var result = new List<StaffAssignmentDto>(assignments.Count);
        foreach (var a in assignments)
        {
            var staffBrief = await _staffUserService.GetUserBriefAsync(a.StaffId, cancellationToken);
            result.Add(new StaffAssignmentDto
            {
                AssignmentId = a.AssignmentId,
                PoolId = a.PoolId,
                PoolName = a.Pool.PoolName,
                StaffId = a.StaffId,
                StaffName = staffBrief?.FullName ?? "Không xác định",
                StaffEmail = staffBrief?.Email ?? string.Empty,
                AssignedAt = a.AssignedAt
            });
        }

        return result;
    }
}
