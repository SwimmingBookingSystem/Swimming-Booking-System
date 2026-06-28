using MediatR;
using Microsoft.EntityFrameworkCore;
using SBS.Application.Common.Dtos.Staff;
using SBS.Application.Common.Interfaces;
using SBS.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Staff.Queries.SearchBookings;

public record StaffSearchBookingsQuery : IRequest<List<BookingListItemDto>>
{
    /// <summary>Tìm chính xác theo mã booking (BookingCode)</summary>
    public string? BookingCode { get; init; }

    /// <summary>Tìm theo số điện thoại khách hàng</summary>
    public string? Phone { get; init; }

    /// <summary>Tìm theo email khách hàng</summary>
    public string? Email { get; init; }
}

public class StaffSearchBookingsQueryHandler : IRequestHandler<StaffSearchBookingsQuery, List<BookingListItemDto>>
{
    private readonly IReadOnlyUnitOfWork _readOnlyUnitOfWork;
    private readonly IStaffUserService _staffUserService;

    public StaffSearchBookingsQueryHandler(
        IReadOnlyUnitOfWork readOnlyUnitOfWork,
        IStaffUserService staffUserService)
    {
        _readOnlyUnitOfWork = readOnlyUnitOfWork;
        _staffUserService = staffUserService;
    }

    public async Task<List<BookingListItemDto>> Handle(StaffSearchBookingsQuery request, CancellationToken cancellationToken)
    {
        var bookingQuery = _readOnlyUnitOfWork.Repository<Booking>()
            .Query()
            .Include(b => b.PoolSlot)
                .ThenInclude(s => s.Pool)
            .AsQueryable();

        // Ưu tiên tìm theo BookingCode trước
        if (!string.IsNullOrWhiteSpace(request.BookingCode))
        {
            bookingQuery = bookingQuery.Where(b => b.BookingCode == request.BookingCode.Trim());
        }
        else if (!string.IsNullOrWhiteSpace(request.Phone) || !string.IsNullOrWhiteSpace(request.Email))
        {
            // Tìm UserId từ IStaffUserService dựa trên phone/email
            var userIds = await _staffUserService.FindUserIdsByPhoneOrEmailAsync(
                request.Phone?.Trim(),
                request.Email?.Trim(),
                cancellationToken);

            if (userIds.Count == 0)
                return new List<BookingListItemDto>();

            bookingQuery = bookingQuery.Where(b => userIds.Contains(b.UserId));
        }
        else
        {
            return new List<BookingListItemDto>();
        }

        var bookings = await _readOnlyUnitOfWork.ToListAsync(
            bookingQuery.OrderByDescending(b => b.CreatedAt).Take(50),
            cancellationToken);

        // Enrich thông tin khách hàng từ IStaffUserService
        var result = new List<BookingListItemDto>(bookings.Count);
        foreach (var b in bookings)
        {
            var customer = await _staffUserService.GetUserBriefAsync(b.UserId, cancellationToken);
            result.Add(new BookingListItemDto
            {
                BookingId = b.BookingId,
                BookingCode = b.BookingCode,
                CustomerName = customer?.FullName ?? "Khách vãng lai",
                CustomerEmail = customer?.Email ?? string.Empty,
                CustomerPhone = customer?.PhoneNumber,
                PoolName = b.PoolSlot.Pool.PoolName,
                SlotTime = $"{b.PoolSlot.StartTime:hh\\:mm} - {b.PoolSlot.EndTime:hh\\:mm}",
                BookingDate = b.BookingDate,
                Status = b.Status,
                TotalAmount = b.TotalAmount,
                BookingType = b.BookingType,
                CreatedAt = b.CreatedAt
            });
        }

        return result;
    }
}
