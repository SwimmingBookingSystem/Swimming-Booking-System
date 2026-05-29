using MediatR;
using Microsoft.EntityFrameworkCore;
using SBS.Application.Common.Interfaces;
using SBS.Application.Common.Models;
using SBS.Application.Features.CheckIn.DTOs;

namespace SBS.Application.Features.CheckIn.Queries;

// ── QUERY ─────────────────────────────────────────────────────────────────────

public record GetBookingsQuery(
    int PoolId,
    DateOnly? Date,
    string? Status,
    int Page,
    int PageSize
) : IRequest<PagedResult<BookingListItemDto>>;

// ── HANDLER ───────────────────────────────────────────────────────────────────

public sealed class GetBookingsQueryHandler
    : IRequestHandler<GetBookingsQuery, PagedResult<BookingListItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public GetBookingsQueryHandler(IApplicationDbContext context, IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task<PagedResult<BookingListItemDto>> Handle(
        GetBookingsQuery request, CancellationToken cancellationToken)
    {
        var targetDate = request.Date ?? DateOnly.FromDateTime(DateTime.UtcNow.Date);

        var query = _context.Bookings
            .Where(b => b.PoolId == request.PoolId && b.BookingDate == targetDate)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Status))
            query = query.Where(b => b.BookingStatus == request.Status);

        var totalCount = await query.CountAsync(cancellationToken);

        var bookings = await query
            .OrderBy(b => b.StartTime)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(b => new
            {
                b.BookingId,
                b.UserId,
                b.BookingDate,
                b.StartTime,
                b.EndTime,
                b.SlotCount,
                b.BookingStatus,
                HasCheckin = b.CustomerCheckins.Any()
            })
            .ToListAsync(cancellationToken);

        // Resolve customer names in parallel
        var items = new List<BookingListItemDto>();
        foreach (var b in bookings)
        {
            string? customerName = b.UserId.HasValue
                ? await _identityService.GetUserFullNameAsync(b.UserId.Value, cancellationToken)
                : null;

            items.Add(new BookingListItemDto
            {
                BookingId = b.BookingId,
                CustomerName = customerName,
                BookingDate = b.BookingDate,
                StartTime = b.StartTime,
                EndTime = b.EndTime,
                SlotCount = b.SlotCount,
                BookingStatus = b.BookingStatus,
                IsCheckedIn = b.HasCheckin
            });
        }

        return new PagedResult<BookingListItemDto>
        {
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            Items = items
        };
    }
}
