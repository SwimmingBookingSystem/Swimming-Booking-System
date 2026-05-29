using MediatR;
using Microsoft.EntityFrameworkCore;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.CheckIn.DTOs;

namespace SBS.Application.Features.CheckIn.Queries;

// ── QUERY ─────────────────────────────────────────────────────────────────────

public record VerifyTicketQuery(string TicketCode) : IRequest<VerifyTicketResponseDto>;

// ── HANDLER ───────────────────────────────────────────────────────────────────

public sealed class VerifyTicketQueryHandler : IRequestHandler<VerifyTicketQuery, VerifyTicketResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public VerifyTicketQueryHandler(IApplicationDbContext context, IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task<VerifyTicketResponseDto> Handle(VerifyTicketQuery request, CancellationToken cancellationToken)
    {
        // Find ticket by code
        var ticket = await _context.Tickets
            .Include(t => t.TicketType)
            .Include(t => t.Booking)
                .ThenInclude(b => b.Pool)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TicketCode == request.TicketCode, cancellationToken);

        if (ticket is null)
        {
            return new VerifyTicketResponseDto
            {
                IsValid = false,
                TicketCode = request.TicketCode,
                Reason = "Mã vé không tồn tại trong hệ thống."
            };
        }

        var booking = ticket.Booking;

        // Check booking status
        if (booking.BookingStatus == "cancelled")
        {
            return new VerifyTicketResponseDto
            {
                IsValid = false,
                TicketCode = request.TicketCode,
                Reason = "Lượt đặt chỗ đã bị hủy."
            };
        }

        // Check if already checked in
        var alreadyCheckedIn = await _context.CustomerCheckins
            .AnyAsync(c => c.BookingId == booking.BookingId, cancellationToken);

        if (alreadyCheckedIn)
        {
            return new VerifyTicketResponseDto
            {
                IsValid = false,
                TicketCode = request.TicketCode,
                Reason = "Vé này đã được sử dụng để check-in."
            };
        }

        // Resolve customer name
        string? customerName = null;
        if (booking.UserId.HasValue)
            customerName = await _identityService.GetUserFullNameAsync(booking.UserId.Value, cancellationToken);

        return new VerifyTicketResponseDto
        {
            IsValid = true,
            TicketId = ticket.TicketId,
            TicketCode = ticket.TicketCode,
            TicketType = ticket.TicketType.TypeName,
            BookingDate = booking.BookingDate,
            StartTime = booking.StartTime,
            EndTime = booking.EndTime,
            PoolName = booking.Pool.PoolName,
            CustomerName = customerName,
            SlotCount = booking.SlotCount,
            BookingStatus = booking.BookingStatus
        };
    }
}
