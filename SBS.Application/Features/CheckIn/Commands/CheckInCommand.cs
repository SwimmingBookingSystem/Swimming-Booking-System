using MediatR;
using Microsoft.EntityFrameworkCore;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.CheckIn.DTOs;
using SBS.Domain.Entities;

namespace SBS.Application.Features.CheckIn.Commands;

// ── COMMAND ───────────────────────────────────────────────────────────────────

/// <param name="StaffId">
/// Staff performing the check-in.
/// TODO: Replace with JWT claim when auth team configures authentication.
/// </param>
public record CheckInCommand(
    string TicketCode,
    int BookingId,
    int StaffId
) : IRequest<CheckInResponseDto>;

// ── HANDLER ───────────────────────────────────────────────────────────────────

public sealed class CheckInCommandHandler : IRequestHandler<CheckInCommand, CheckInResponseDto>
{
    private readonly IApplicationDbContext _context;

    public CheckInCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CheckInResponseDto> Handle(CheckInCommand request, CancellationToken cancellationToken)
    {
        // Verify booking exists
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.BookingId == request.BookingId, cancellationToken)
            ?? throw new InvalidOperationException($"Không tìm thấy booking với ID {request.BookingId}.");

        // Guard: booking must not be cancelled
        if (booking.BookingStatus == "cancelled")
            throw new InvalidOperationException("Lượt đặt chỗ đã bị hủy, không thể check-in.");

        // Guard: verify ticket code belongs to this booking
        var ticketExists = await _context.Tickets
            .AnyAsync(t => t.TicketCode == request.TicketCode && t.BookingId == request.BookingId,
                cancellationToken);

        if (!ticketExists)
            throw new InvalidOperationException("Mã vé không hợp lệ hoặc không thuộc booking này.");

        // Guard: check for duplicate check-in
        var alreadyCheckedIn = await _context.CustomerCheckins
            .AnyAsync(c => c.BookingId == request.BookingId, cancellationToken);

        if (alreadyCheckedIn)
            throw new InvalidOperationException("Khách hàng đã check-in cho lượt đặt chỗ này rồi.");

        // Determine userId: use booking's customer, fallback to staffId for walk-ins
        var userId = booking.UserId ?? request.StaffId;

        var checkin = new CustomerCheckin
        {
            UserId = userId,
            BookingId = request.BookingId,
            CheckinStatus = 1,  // 1 = checked_in
            CheckinTime = DateTime.UtcNow
        };

        _context.CustomerCheckins.Add(checkin);
        await _context.SaveChangesAsync(cancellationToken);

        return new CheckInResponseDto
        {
            CheckinId = checkin.CheckinId,
            BookingId = checkin.BookingId,
            CheckinTime = checkin.CheckinTime!.Value,
            CheckinStatus = "checked_in",
            Message = "Check-in thành công. Chào mừng khách hàng vào bể bơi!"
        };
    }
}
