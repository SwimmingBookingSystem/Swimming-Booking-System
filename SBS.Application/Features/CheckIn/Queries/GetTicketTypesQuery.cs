using MediatR;
using Microsoft.EntityFrameworkCore;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.CheckIn.DTOs;

namespace SBS.Application.Features.CheckIn.Queries;

// ── QUERY ─────────────────────────────────────────────────────────────────────

public record GetTicketTypesQuery(int PoolId) : IRequest<IReadOnlyList<TicketTypeDto>>;

// ── HANDLER ───────────────────────────────────────────────────────────────────

public sealed class GetTicketTypesQueryHandler
    : IRequestHandler<GetTicketTypesQuery, IReadOnlyList<TicketTypeDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTicketTypesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<TicketTypeDto>> Handle(
        GetTicketTypesQuery request, CancellationToken cancellationToken)
    {
        var ticketTypes = await _context.PoolTicketTypes
            .Where(pt => pt.PoolId == request.PoolId && pt.Status == "active")
            .Include(pt => pt.TicketType)
            .AsNoTracking()
            .Select(pt => new TicketTypeDto
            {
                TicketTypeId = pt.TicketType.TicketTypeId,
                TypeCode = pt.TicketType.TypeCode,
                TypeName = pt.TicketType.TypeName,
                Description = pt.TicketType.Description,
                BasePrice = pt.TicketType.BasePrice,
                IsCombo = pt.TicketType.IsCombo ?? false,
                DiscountPercent = pt.TicketType.DiscountPercent
            })
            .OrderBy(t => t.BasePrice)
            .ToListAsync(cancellationToken);

        return ticketTypes;
    }
}
