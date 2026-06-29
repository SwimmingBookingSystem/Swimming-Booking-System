using MediatR;
using Microsoft.EntityFrameworkCore;
using SBS.Application.Common.Dtos;
using SBS.Application.Common.Dtos.Staff;
using SBS.Application.Common.Interfaces;
using SBS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Staff.Queries.GetAllBookings;

public record StaffGetAllBookingsQuery : IRequest<PagedResultDto<BookingListItemDto>>
{
    /// <summary>Filter theo trạng thái: PendingPayment, Confirmed, CheckIn, Cancelled, NoShow, Waitlist</summary>
    public string? Status { get; init; }

    /// <summary>Filter theo ngày bơi</summary>
    public DateOnly? BookingDate { get; init; }

    /// <summary>Filter theo bể bơi</summary>
    public int? PoolId { get; init; }

    /// <summary>Filter theo loại booking: Online, WalkIn</summary>
    public string? BookingType { get; init; }

    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public class StaffGetAllBookingsQueryHandler : IRequestHandler<StaffGetAllBookingsQuery, PagedResultDto<BookingListItemDto>>
{
    private readonly IReadOnlyUnitOfWork _readOnlyUnitOfWork;
    private readonly IStaffUserService _staffUserService;
    private readonly ICurrentUserService _currentUserService;

    public StaffGetAllBookingsQueryHandler(
        IReadOnlyUnitOfWork readOnlyUnitOfWork,
        IStaffUserService staffUserService,
        ICurrentUserService currentUserService)
    {
        _readOnlyUnitOfWork = readOnlyUnitOfWork;
        _staffUserService = staffUserService;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResultDto<BookingListItemDto>> Handle(StaffGetAllBookingsQuery request, CancellationToken cancellationToken)
    {
        // Guard: lấy danh sách Pool được phân công cho Staff đang đăng nhập
        var staffId = Guid.TryParse(_currentUserService.UserId, out var sid) ? sid : Guid.Empty;
        var assignedPoolIds = await _staffUserService.GetAssignedPoolIdsAsync(staffId, cancellationToken);

        var query = _readOnlyUnitOfWork.Repository<Booking>()
            .Query()
            .Include(b => b.PoolSlot)
                .ThenInclude(s => s.Pool)
            .AsQueryable();

        // Giới hạn theo hồ bơi được phân công
        if (assignedPoolIds.Count > 0)
            query = query.Where(b => assignedPoolIds.Contains(b.PoolSlot.PoolId));
        else
            // Staff chưa được phân công hồ nào → trả về rỗng
            return new PagedResultDto<BookingListItemDto> { Items = new(), TotalCount = 0, Page = request.Page, PageSize = request.PageSize };

        // Apply filters
        if (!string.IsNullOrEmpty(request.Status))
            query = query.Where(b => b.Status == request.Status);

        if (request.BookingDate.HasValue)
            query = query.Where(b => b.BookingDate == request.BookingDate.Value);

        if (request.PoolId.HasValue)
            query = query.Where(b => b.PoolSlot.PoolId == request.PoolId.Value);

        if (!string.IsNullOrEmpty(request.BookingType))
            query = query.Where(b => b.BookingType == request.BookingType);

        // Tổng số bản ghi
        var totalCount = await _readOnlyUnitOfWork.CountAsync(query, cancellationToken);

        // Phân trang + sắp xếp mới nhất trước
        var pagedQuery = query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize);

        var bookings = await _readOnlyUnitOfWork.ToListAsync(pagedQuery, cancellationToken);

        // Enrich thông tin khách hàng từ IStaffUserService
        var items = new List<BookingListItemDto>(bookings.Count);
        foreach (var b in bookings)
        {
            var customer = await _staffUserService.GetUserBriefAsync(b.UserId, cancellationToken);
            items.Add(new BookingListItemDto
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

        return new PagedResultDto<BookingListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
