using MediatR;
using Microsoft.EntityFrameworkCore;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.CheckIn.DTOs;

namespace SBS.Application.Features.CheckIn.Queries;

// ── QUERY ─────────────────────────────────────────────────────────────────────

public record GetBookingDetailQuery(int BookingId) : IRequest<BookingDetailDto?>;

// ── HANDLER ───────────────────────────────────────────────────────────────────

public sealed class GetBookingDetailQueryHandler : IRequestHandler<GetBookingDetailQuery, BookingDetailDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public GetBookingDetailQueryHandler(IApplicationDbContext context, IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task<BookingDetailDto?> Handle(GetBookingDetailQuery request, CancellationToken cancellationToken)
    {
        var booking = await _context.Bookings
            .Include(b => b.Pool)
            .Include(b => b.Discount)
            .Include(b => b.Tickets)
                .ThenInclude(t => t.TicketType)
            .Include(b => b.BookingServices)
                .ThenInclude(bs => bs.PoolService)
            .Include(b => b.CustomerCheckins)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.BookingId == request.BookingId, cancellationToken);

        if (booking is null) return null;

        string? customerName = null;
        string? customerPhone = null;
        if (booking.UserId.HasValue)
        {
            customerName = await _identityService.GetUserFullNameAsync(booking.UserId.Value, cancellationToken);
            customerPhone = await _identityService.GetUserPhoneAsync(booking.UserId.Value, cancellationToken);
        }

        return new BookingDetailDto
        {
            BookingId = booking.BookingId,
            UserId = booking.UserId,
            CustomerName = customerName,
            CustomerPhone = customerPhone,
            PoolId = booking.PoolId,
            PoolName = booking.Pool.PoolName,
            BookingDate = booking.BookingDate,
            StartTime = booking.StartTime,
            EndTime = booking.EndTime,
            SlotCount = booking.SlotCount,
            BookingStatus = booking.BookingStatus,
            DiscountCode = booking.Discount?.DiscountCode,
            Tickets = booking.Tickets.Select(t => new BookingTicketItemDto
            {
                TicketId = t.TicketId,
                TicketCode = t.TicketCode,
                TicketType = t.TicketType.TypeName,
                TicketPrice = t.TicketPrice,
                IssuedAt = t.IssuedAt
            }).ToList(),
            BookingServices = booking.BookingServices.Select(bs => new BookingServiceItemDto
            {
                BookingServiceId = bs.BookingServiceId,
                ServiceName = bs.PoolService.ServiceName,
                Quantity = bs.Quantity,
                TotalServicePrice = bs.TotalServicePrice
            }).ToList(),
            Checkins = booking.CustomerCheckins.Select(c => new BookingCheckinItemDto
            {
                CheckinId = c.CheckinId,
                CheckinTime = c.CheckinTime,
                CheckinStatus = c.CheckinStatus.HasValue
                    ? (c.CheckinStatus == 1 ? "checked_in" : "pending")
                    : "pending"
            }).ToList()
        };
    }
}
