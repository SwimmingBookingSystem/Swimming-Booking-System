using MediatR;
using Microsoft.EntityFrameworkCore;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.ServiceStaff.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.ServiceStaff.Queries;

// ── QUERY ─────────────────────────────────────────────────────────────────────

public record GetBookingServicesQuery(int BookingId) : IRequest<BookingServicesDto>;

// ── HANDLER ───────────────────────────────────────────────────────────────────

public sealed class GetBookingServicesQueryHandler : IRequestHandler<GetBookingServicesQuery, BookingServicesDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public GetBookingServicesQueryHandler(IApplicationDbContext context, IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task<BookingServicesDto> Handle(GetBookingServicesQuery request, CancellationToken cancellationToken)
    {
        var booking = await _context.Bookings
            .Include(b => b.BookingServices)
                .ThenInclude(bs => bs.PoolService)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.BookingId == request.BookingId, cancellationToken)
            ?? throw new KeyNotFoundException($"Không tìm thấy lượt đặt chỗ với ID {request.BookingId}.");

        // Resolve customer name
        string? customerName = null;
        if (booking.UserId.HasValue)
        {
            customerName = await _identityService.GetUserFullNameAsync(booking.UserId.Value, cancellationToken);
        }
        else
        {
            // If direct sale walk-in, we can try to look up customerName from SaleTicketDirectlys
            var sale = await _context.SaleTicketDirectlys
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.BookingId == booking.BookingId, cancellationToken);
            if (sale != null)
                customerName = sale.CustomerName;
        }

        var result = new BookingServicesDto
        {
            BookingId = booking.BookingId,
            CustomerName = customerName ?? "Khách vãng lai",
            BookingDate = booking.BookingDate,
            StartTime = booking.StartTime,
            EndTime = booking.EndTime,
            Services = booking.BookingServices.Select(bs => new BookingServiceItemDto
            {
                BookingServiceId = bs.BookingServiceId,
                PoolServiceId = bs.PoolServiceId,
                ServiceName = bs.PoolService.ServiceName,
                Quantity = bs.Quantity ?? 0,
                TotalServicePrice = bs.TotalServicePrice
            }).ToList()
        };

        return result;
    }
}
