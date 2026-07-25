using MediatR;
using Microsoft.EntityFrameworkCore;
using SBS.Application.Common.Dtos;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.Customer_Bookings.Dtos;
using SBS.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Customer_Bookings.Queries.GetCustomerBookings;

public record GetCustomerBookingsQuery(int PageNumber = 1, int PageSize = 10)
    : IRequest<PagedResultDto<CustomerBookingHistoryDto>>;

public class GetCustomerBookingsQueryHandler
    : IRequestHandler<GetCustomerBookingsQuery, PagedResultDto<CustomerBookingHistoryDto>>
{
    private readonly IReadOnlyUnitOfWork _readUnitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetCustomerBookingsQueryHandler(IReadOnlyUnitOfWork readUnitOfWork, ICurrentUserService currentUserService)
    {
        _readUnitOfWork = readUnitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResultDto<CustomerBookingHistoryDto>> Handle(
        GetCustomerBookingsQuery request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var userId))
        {
            throw new UnauthorizedAccessException("Người dùng chưa đăng nhập hoặc ID không hợp lệ.");
        }

        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 50);
        var repository = _readUnitOfWork.Repository<Booking>();
        var query = repository.Query()
            .AsNoTracking()
            .Where(b => b.UserId == userId);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
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

        return new PagedResultDto<CustomerBookingHistoryDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pageNumber,
            PageSize = pageSize
        };
    }
}