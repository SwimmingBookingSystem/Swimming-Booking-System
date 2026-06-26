using MediatR;
using SBS.Application.Common.Dtos.Manager;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.Manager.TicketTypes.Dtos;
using SBS.Domain.Entities;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Manager.TicketTypes.Queries.GetTicketTypes;

// ── Query 
public record GetTicketTypesQuery(
    int Page = 1,
    int PageSize = 10,
    string? Category = null,
    string? Status = null
) : IRequest<PagedResponse<TicketTypeDto>>;

// ── Handler 
public class GetTicketTypesQueryHandler
    : IRequestHandler<GetTicketTypesQuery, PagedResponse<TicketTypeDto>>
{
    private readonly IUnitOfWork _uow;

    public GetTicketTypesQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<PagedResponse<TicketTypeDto>> Handle(
        GetTicketTypesQuery request, CancellationToken ct)
    {
        var query = _uow.Repository<TicketType>().Query();

        if (!string.IsNullOrEmpty(request.Category))
            query = query.Where(t => t.Category == request.Category);

        if (!string.IsNullOrEmpty(request.Status))
            query = query.Where(t => t.Status == request.Status);

        var total = await _uow.CountAsync(query, ct);

        var items = await _uow.ToListAsync(
            query.OrderBy(t => t.Category)
                 .ThenBy(t => t.TicketCode)
                 .Skip((request.Page - 1) * request.PageSize)
                 .Take(request.PageSize)
                 .Select(t => new TicketTypeDto
                 {
                     TicketTypeId    = t.TicketTypeId,
                     TicketCode      = t.TicketCode,
                     TicketName      = t.TicketName,
                     Category        = t.Category,
                     BasePrice       = t.BasePrice,
                     DiscountPercent = t.DiscountPercent,
                     Description     = t.Description,
                     Status          = t.Status,
                     CreatedAt       = t.CreatedAt
                 }), ct);

        return new PagedResponse<TicketTypeDto>
        {
            Items      = items,
            TotalCount = total,
            Page       = request.Page,
            PageSize   = request.PageSize
        };
    }
}
