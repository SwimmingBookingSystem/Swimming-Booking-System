using MediatR;
using Microsoft.EntityFrameworkCore;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.Customer_Bookings.Dtos;
using SBS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Customer_Bookings.Queries.GetCustomerBookings;

public record GetCustomerBookingsQuery() : IRequest<List<CustomerBookingHistoryDto>>;

public class GetCustomerBookingsQueryHandler : IRequestHandler<GetCustomerBookingsQuery, List<CustomerBookingHistoryDto>>
{
    private readonly IReadOnlyUnitOfWork _readUnitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetCustomerBookingsQueryHandler(IReadOnlyUnitOfWork readUnitOfWork, ICurrentUserService currentUserService)
    {
        _readUnitOfWork = readUnitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<List<CustomerBookingHistoryDto>> Handle(GetCustomerBookingsQuery request, CancellationToken cancellationToken)
    {
        var userIdString = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            throw new UnauthorizedAccessException("Người dùng chưa đăng nhập hoặc ID không hợp lệ.");
        }

        var repository = _readUnitOfWork.Repository<Booking>();

        var bookings = await repository.Query()
            .AsNoTracking()
            .Include(b => b.PoolSlot)
                .ThenInclude(ps => ps.Pool)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new CustomerBookingHistoryDto
            {
                BookingId = b.BookingId,
                BookingCode = b.BookingCode,
                PoolName = b.PoolSlot.Pool.PoolName,
                SlotDate = b.PoolSlot.SlotDate,
                StartTime = b.PoolSlot.StartTime,
                EndTime = b.PoolSlot.EndTime,
                Status = b.Status,
                TotalAmount = b.TotalAmount,
                QrCodeData = b.QrCodeData,
                CreatedAt = b.CreatedAt,
                Tickets = b.BookingDetails.Select(bd => new CustomerBookingHistoryTicketDto
                {
                    TicketName = bd.PoolTicketType.TicketType.TicketName,
                    Category = bd.PoolTicketType.TicketType.Category,
                    Quantity = bd.Quantity
                }).ToList()
            })
            .ToListAsync(cancellationToken);

        return bookings;
    }
}
