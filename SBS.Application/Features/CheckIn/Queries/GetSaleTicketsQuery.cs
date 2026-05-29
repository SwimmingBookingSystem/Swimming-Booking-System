using MediatR;
using Microsoft.EntityFrameworkCore;
using SBS.Application.Common.Interfaces;
using SBS.Application.Common.Models;
using SBS.Application.Features.CheckIn.DTOs;

namespace SBS.Application.Features.CheckIn.Queries;

// ── QUERY ─────────────────────────────────────────────────────────────────────

public record GetSaleTicketsQuery(
    int PoolId,
    DateOnly? Date,
    int Page,
    int PageSize
) : IRequest<PagedResult<SaleTicketListItemDto>>;

// ── HANDLER ───────────────────────────────────────────────────────────────────

public sealed class GetSaleTicketsQueryHandler
    : IRequestHandler<GetSaleTicketsQuery, PagedResult<SaleTicketListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSaleTicketsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<SaleTicketListItemDto>> Handle(
        GetSaleTicketsQuery request, CancellationToken cancellationToken)
    {
        var targetDate = request.Date ?? DateOnly.FromDateTime(DateTime.UtcNow.Date);

        var query = _context.SaleTicketDirectlys
            .Include(s => s.Booking)
            .Where(s => s.Booking.PoolId == request.PoolId
                     && DateOnly.FromDateTime(s.SaleDate.Date) == targetDate)
            .AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(s => s.SaleDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new SaleTicketListItemDto
            {
                SaleId = s.SaleId,
                TicketCode = s.Booking.Tickets
                    .OrderBy(t => t.IssuedAt)
                    .Select(t => t.TicketCode)
                    .FirstOrDefault() ?? string.Empty,
                CustomerName = s.CustomerName,
                TotalAmount = s.TotalAmount,
                PaymentMethod = s.PaymentMethod,
                PaymentStatus = s.PaymentStatus,
                SaleDate = s.SaleDate
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<SaleTicketListItemDto>
        {
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            Items = items
        };
    }
}
