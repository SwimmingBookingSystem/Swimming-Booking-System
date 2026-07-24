using System;
using MediatR;
using SBS.Application.Common.Dtos;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.Contacts.Dtos;
using SBS.Domain.Entities;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Contacts.Queries.GetMyContactHistory;

public class GetMyContactHistoryQueryHandler : IRequestHandler<GetMyContactHistoryQuery, PagedResultDto<ContactHistoryDto>>
{
    private readonly IReadOnlyUnitOfWork _readOnlyUow;

    public GetMyContactHistoryQueryHandler(IReadOnlyUnitOfWork readOnlyUow)
    {
        _readOnlyUow = readOnlyUow;
    }

    public async Task<PagedResultDto<ContactHistoryDto>> Handle(GetMyContactHistoryQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 50);
        var repository = _readOnlyUow.Repository<ContactRequest>();
        var query = repository.Query()
            .Where(x => x.UserId == request.UserId);

        var totalCount = await _readOnlyUow.CountAsync(query, cancellationToken);

        var paginatedQuery = query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);

        var entities = await _readOnlyUow.ToListAsync(paginatedQuery, cancellationToken);

        var items = entities.Select(x => new ContactHistoryDto
        {
            ContactRequestId = x.ContactRequestId,
            FullName = x.FullName,
            Email = x.Email,
            Category = x.Category,
            Message = x.Message,
            Status = x.Status,
            CreatedAt = x.CreatedAt,
            HandledAt = x.HandledAt
        }).ToList();

        return new PagedResultDto<ContactHistoryDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pageNumber,
            PageSize = pageSize
        };
    }
}
