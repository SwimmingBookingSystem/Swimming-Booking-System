using MediatR;
using SBS.Application.Common.Interfaces;
using SBS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Admin.Queries.GetContactRequests;

public record GetContactRequestsQuery : IRequest<List<ContactRequestListDto>>;

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

public class GetContactRequestsQueryHandler : IRequestHandler<GetContactRequestsQuery, List<ContactRequestListDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetContactRequestsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<ContactRequestListDto>> Handle(GetContactRequestsQuery request, CancellationToken cancellationToken)
    {
        var contacts = await _unitOfWork.ToListAsync(
            _unitOfWork.Repository<ContactRequest>()
                .Query()
                .OrderByDescending(c => c.CreatedAt),
            cancellationToken);

        return contacts.Select(c => new ContactRequestListDto
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
    }
}
