using MediatR;
using SBS.Application.Common.Dtos.Staff;
using SBS.Application.Common.Interfaces;
using SBS.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Staff.Queries.GetPendingContactRequests;

public record StaffGetPendingContactRequestsQuery : IRequest<List<ContactRequestDto>>
{
    /// <summary>
    /// Filter theo trạng thái: "Pending", "Resolved", hoặc null = tất cả.
    /// Mặc định Staff xem danh sách Pending.
    /// </summary>
    public string? Status { get; init; } = "Pending";
}

public class StaffGetPendingContactRequestsQueryHandler : IRequestHandler<StaffGetPendingContactRequestsQuery, List<ContactRequestDto>>
{
    private readonly IReadOnlyUnitOfWork _readOnlyUnitOfWork;

    public StaffGetPendingContactRequestsQueryHandler(IReadOnlyUnitOfWork readOnlyUnitOfWork)
    {
        _readOnlyUnitOfWork = readOnlyUnitOfWork;
    }

    public async Task<List<ContactRequestDto>> Handle(StaffGetPendingContactRequestsQuery request, CancellationToken cancellationToken)
    {
        var query = _readOnlyUnitOfWork.Repository<ContactRequest>()
            .Query()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Status) && request.Status != "All")
            query = query.Where(c => c.Status == request.Status);

        query = query.OrderByDescending(c => c.CreatedAt);

        var contacts = await _readOnlyUnitOfWork.ToListAsync(query, cancellationToken);

        return contacts.Select(c => new ContactRequestDto
        {
            ContactRequestId = c.ContactRequestId,
            Email = c.Email,
            Reason = c.Reason,
            Status = c.Status,
            CreatedAt = c.CreatedAt,
            HandledAt = c.HandledAt
        }).ToList();
    }
}
