using MediatR;
using SBS.Application.Common.Dtos;
using SBS.Application.Common.Interfaces;
using SBS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Admin.Queries.GetContactRequests;

public record GetContactRequestsQuery(int Page = 1, int PageSize = 10, string? Status = null) : IRequest<PagedResultDto<ContactRequestListDto>>;

public class ContactRequestListDto
{
    public int ContactRequestId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string Category { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public class GetContactRequestsQueryHandler : IRequestHandler<GetContactRequestsQuery, PagedResultDto<ContactRequestListDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetContactRequestsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResultDto<ContactRequestListDto>> Handle(GetContactRequestsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<ContactRequest>()
            .Query();

        if (!string.IsNullOrWhiteSpace(request.Status))
            query = query.Where(c => c.Status == request.Status);

        var totalCount = await _unitOfWork.CountAsync(query, cancellationToken);

        var paginatedQuery = query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize);

        var contacts = await _unitOfWork.ToListAsync(paginatedQuery, cancellationToken);

        var items = contacts.Select(c => new ContactRequestListDto
        {
            ContactRequestId = c.ContactRequestId,
            FullName = c.FullName,
            Email = c.Email,
            PhoneNumber = c.PhoneNumber,
            Category = c.Category,
            Message = c.Message,
            Status = c.Status,
            CreatedAt = c.CreatedAt
        }).ToList();

        return new PagedResultDto<ContactRequestListDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
