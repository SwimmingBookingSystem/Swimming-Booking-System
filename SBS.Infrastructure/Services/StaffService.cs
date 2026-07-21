using MassTransit;
using Microsoft.EntityFrameworkCore;
using SBS.Application.Common;
using SBS.Application.Common.Dtos;
using SBS.Application.Common.Dtos.Staff;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.Customer_Bookings.Events;
using SBS.Application.Features.Staff.Commands;
using SBS.Application.Features.Staff.Commands.CheckOut;
using SBS.Application.Features.Staff.Commands.ManualCheckIn;
using SBS.Application.Features.Staff.Commands.QrCheckIn;
using SBS.Application.Features.Staff.Queries.GetAllBookings;
using SBS.Application.Features.Staff.Queries.GetBookingDetail;
using SBS.Application.Features.Staff.Queries.SearchBookings;
using SBS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Infrastructure.Services;

public class StaffService : IStaffService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IReadOnlyUnitOfWork _readOnlyUnitOfWork;
    private readonly IStaffUserService _staffUserService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPublishEndpoint _publishEndpoint;

    public StaffService(
        IUnitOfWork unitOfWork,
        IReadOnlyUnitOfWork readOnlyUnitOfWork,
        IStaffUserService staffUserService,
        ICurrentUserService currentUserService,
        IPublishEndpoint publishEndpoint)
    {
        _unitOfWork = unitOfWork;
        _readOnlyUnitOfWork = readOnlyUnitOfWork;
        _staffUserService = staffUserService;
        _currentUserService = currentUserService;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<List<BookingListItemDto>> SearchBookingsAsync(StaffSearchBookingsQuery request, CancellationToken cancellationToken = default)
    {
        var bookingQuery = _readOnlyUnitOfWork.Repository<Booking>()
            .Query()
            .Include(b => b.PoolSlot)
                .ThenInclude(s => s.Pool)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.BookingCode))
        {
            bookingQuery = bookingQuery.Where(b => b.BookingCode == request.BookingCode.Trim());
        }
        else if (!string.IsNullOrWhiteSpace(request.Phone) || !string.IsNullOrWhiteSpace(request.Email))
        {
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

        var bookingUserIds = bookings.Select(b => b.UserId).Distinct();
        var userMap = await _staffUserService.GetUserBriefsByIdsAsync(bookingUserIds, cancellationToken);

        var result = new List<BookingListItemDto>(bookings.Count);
        foreach (var b in bookings)
        {
            userMap.TryGetValue(b.UserId, out var customer);
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
                StatusDisplay = BookingStatus.ToDisplayName(b.Status),
                TotalAmount = b.TotalAmount,
                BookingType = b.BookingType,
                CreatedAt = b.CreatedAt
            });
        }

        return result;
    }

    public async Task<PagedResultDto<BookingListItemDto>> GetAllBookingsAsync(StaffGetAllBookingsQuery request, CancellationToken cancellationToken = default)
    {
        var staffId = Guid.TryParse(_currentUserService.UserId, out var sid) ? sid : Guid.Empty;
        var assignedPoolIds = await _staffUserService.GetAssignedPoolIdsAsync(staffId, cancellationToken);

        var query = _readOnlyUnitOfWork.Repository<Booking>()
            .Query()
            .Include(b => b.PoolSlot)
                .ThenInclude(s => s.Pool)
            .AsQueryable();

        if (assignedPoolIds.Count > 0)
            query = query.Where(b => assignedPoolIds.Contains(b.PoolSlot.PoolId));
        else
            return new PagedResultDto<BookingListItemDto> { Items = new(), TotalCount = 0, Page = request.Page, PageSize = request.PageSize };

        if (!string.IsNullOrEmpty(request.Status))
            query = query.Where(b => b.Status == request.Status);

        query = query.Where(b => b.BookingDate == DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7)));

        if (request.PoolId.HasValue)
            query = query.Where(b => b.PoolSlot.PoolId == request.PoolId.Value);

        if (!string.IsNullOrEmpty(request.BookingType))
            query = query.Where(b => b.BookingType == request.BookingType);

        var totalCount = await _readOnlyUnitOfWork.CountAsync(query, cancellationToken);

        var pagedQuery = query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize);

        var bookings = await _readOnlyUnitOfWork.ToListAsync(pagedQuery, cancellationToken);

        var userIds = bookings.Select(b => b.UserId).Distinct();
        var userMap = await _staffUserService.GetUserBriefsByIdsAsync(userIds, cancellationToken);

        var items = new List<BookingListItemDto>(bookings.Count);
        foreach (var b in bookings)
        {
            userMap.TryGetValue(b.UserId, out var customer);
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
                StatusDisplay = BookingStatus.ToDisplayName(b.Status),
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

    public async Task<BookingDetailDto?> GetBookingDetailAsync(StaffGetBookingDetailQuery request, CancellationToken cancellationToken = default)
    {
        var booking = await _readOnlyUnitOfWork.FirstOrDefaultAsync(
            _readOnlyUnitOfWork.Repository<Booking>()
                .Query()
                .Include(b => b.PoolSlot)
                    .ThenInclude(s => s.Pool)
                .Include(b => b.BookingDetails)
                    .ThenInclude(d => d.PoolTicketType)
                        .ThenInclude(pt => pt.TicketType)
                .Include(b => b.Payment)
                .Include(b => b.CheckIn)
                .Where(b => b.BookingId == request.BookingId),
            cancellationToken);

        if (booking is null)
            return null;

        var customer = await _staffUserService.GetUserBriefAsync(booking.UserId, cancellationToken);

        return new BookingDetailDto
        {
            BookingId = booking.BookingId,
            BookingCode = booking.BookingCode,
            Status = booking.Status,
            StatusDisplay = BookingStatus.ToDisplayName(booking.Status),
            BookingDate = booking.BookingDate,
            TotalAmount = booking.TotalAmount,
            BookingType = booking.BookingType,
            PaymentDeadline = booking.PaymentDeadline,
            CreatedAt = booking.CreatedAt,

            CustomerId = booking.UserId,
            CustomerName = customer?.FullName ?? "Khách vãng lai",
            CustomerEmail = customer?.Email ?? string.Empty,
            CustomerPhone = customer?.PhoneNumber,

            PoolName = booking.PoolSlot.Pool.PoolName,
            SlotTime = $"{booking.PoolSlot.StartTime:hh\\:mm} - {booking.PoolSlot.EndTime:hh\\:mm}",

            Tickets = booking.BookingDetails.Select(d => new BookingTicketItemDto
            {
                TicketName = d.PoolTicketType.TicketType.TicketName,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                SubTotal = d.SubTotal
            }).ToList(),

            Payment = booking.Payment is null ? null : new PaymentInfoDto
            {
                PaymentMethod = booking.Payment.PaymentMethod,
                Status = booking.Payment.Status,
                StatusDisplay = PaymentStatus.ToDisplayName(booking.Payment.Status),
                Amount = booking.Payment.Amount,
                PaymentDate = booking.Payment.PaymentDate
            },

            CheckIn = booking.CheckIn is null ? null : new CheckInInfoDto
            {
                CheckInMethod = booking.CheckIn.CheckInMethod,
                CheckInTime = booking.CheckIn.CheckInTime
            }
        };
    }

    public async Task<StaffCheckInResultDto> QrCheckInAsync(StaffQrCheckInCommand request, CancellationToken cancellationToken = default)
    {
        var staffIdString = _currentUserService.UserId;
        if (string.IsNullOrEmpty(staffIdString) || !Guid.TryParse(staffIdString, out var staffId))
            return new StaffCheckInResultDto { Succeeded = false, Message = "Nhân viên chưa đăng nhập hoặc không hợp lệ." };

        var bookingRepo = _unitOfWork.Repository<Booking>();
        var trimCode = request.BookingCode.Trim();
        var booking = await _unitOfWork.FirstOrDefaultAsync(
            bookingRepo.Query()
                .Include(b => b.CheckIn)
                .Include(b => b.PoolSlot)
                .Where(b => b.QrCodeData == trimCode || b.BookingCode == trimCode),
            cancellationToken);

        if (booking is null)
            return new StaffCheckInResultDto { Succeeded = false, Message = "Không tìm thấy booking với mã QR này." };

        var isAssigned = await _staffUserService.IsStaffAssignedToPoolAsync(staffId, booking.PoolSlot.PoolId, cancellationToken);
        if (!isAssigned)
            return new StaffCheckInResultDto { Succeeded = false, Message = "Bạn không có quyền check-in tại hồ bơi này." };

        var localNow = DateTime.UtcNow.AddHours(7);
        var (isValid, errorDto) = CheckInValidationHelper.Validate(booking, localNow);
        if (!isValid)
            return errorDto!;

        var customerBrief = await _staffUserService.GetUserBriefAsync(booking.UserId, cancellationToken);

        var checkIn = new CheckIn
        {
            BookingId = booking.BookingId,
            CheckedByUserId = staffId,
            CheckInMethod = "QR",
            CheckInTime = DateTime.UtcNow
        };
        await _unitOfWork.Repository<CheckIn>().AddAsync(checkIn, cancellationToken);

        booking.Status = BookingStatus.CheckIn;
        booking.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Repository<Booking>().Update(booking);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var slotTime = $"{booking.PoolSlot.StartTime:hh\\:mm} - {booking.PoolSlot.EndTime:hh\\:mm}";

        return new StaffCheckInResultDto
        {
            Succeeded = true,
            Message = "Check-in thành công.",
            CustomerName = customerBrief?.FullName ?? "Khách vãng lai",
            SlotTime = slotTime
        };
    }

    public async Task<StaffCheckInResultDto> ManualCheckInAsync(StaffManualCheckInCommand request, CancellationToken cancellationToken = default)
    {
        var staffIdString = _currentUserService.UserId;
        if (string.IsNullOrEmpty(staffIdString) || !Guid.TryParse(staffIdString, out var staffId))
            return new StaffCheckInResultDto { Succeeded = false, Message = "Nhân viên chưa đăng nhập hoặc không hợp lệ." };

        var bookingRepo = _unitOfWork.Repository<Booking>();
        var query = bookingRepo.Query()
            .Include(b => b.CheckIn)
            .Include(b => b.PoolSlot)
            .AsQueryable();

        if (request.BookingId.HasValue && request.BookingId.Value > 0)
        {
            query = query.Where(b => b.BookingId == request.BookingId.Value);
        }
        else if (!string.IsNullOrWhiteSpace(request.BookingCode))
        {
            var trimCode = request.BookingCode.Trim();
            query = query.Where(b => b.BookingCode == trimCode || b.QrCodeData == trimCode);
        }
        else
        {
            return new StaffCheckInResultDto { Succeeded = false, Message = "Vui lòng cung cấp BookingId hoặc BookingCode." };
        }

        var booking = await _unitOfWork.FirstOrDefaultAsync(query, cancellationToken);

        if (booking is null)
            return new StaffCheckInResultDto { Succeeded = false, Message = "Không tìm thấy booking." };

        var isAssigned = await _staffUserService.IsStaffAssignedToPoolAsync(staffId, booking.PoolSlot.PoolId, cancellationToken);
        if (!isAssigned)
            return new StaffCheckInResultDto { Succeeded = false, Message = "Bạn không có quyền check-in tại hồ bơi này." };

        var localNow = DateTime.UtcNow.AddHours(7);
        var (isValid, errorDto) = CheckInValidationHelper.Validate(booking, localNow);
        if (!isValid)
            return errorDto!;

        var customerBrief = await _staffUserService.GetUserBriefAsync(booking.UserId, cancellationToken);

        var checkIn = new CheckIn
        {
            BookingId = booking.BookingId,
            CheckedByUserId = staffId,
            CheckInMethod = "Manual",
            CheckInTime = DateTime.UtcNow
        };
        await _unitOfWork.Repository<CheckIn>().AddAsync(checkIn, cancellationToken);

        booking.Status = BookingStatus.CheckIn;
        booking.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Repository<Booking>().Update(booking);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var slotTime = $"{booking.PoolSlot.StartTime:hh\\:mm} - {booking.PoolSlot.EndTime:hh\\:mm}";

        return new StaffCheckInResultDto
        {
            Succeeded = true,
            Message = "Check-in thủ công thành công.",
            CustomerName = customerBrief?.FullName ?? "Khách vãng lai",
            SlotTime = slotTime
        };
    }

    public async Task<StaffCheckOutResultDto> CheckOutAsync(StaffCheckOutCommand request, CancellationToken cancellationToken = default)
    {
        var staffIdString = _currentUserService.UserId;
        if (string.IsNullOrEmpty(staffIdString) || !Guid.TryParse(staffIdString, out var staffId))
            return new StaffCheckOutResultDto { Succeeded = false, Message = "Nhân viên chưa đăng nhập hoặc không hợp lệ." };

        var bookingRepo = _unitOfWork.Repository<Booking>();
        var query = bookingRepo.Query()
            .Include(b => b.PoolSlot)
            .Include(b => b.CheckIn)
            .AsQueryable();

        if (request.BookingId.HasValue && request.BookingId.Value > 0)
        {
            query = query.Where(b => b.BookingId == request.BookingId.Value);
        }
        else if (!string.IsNullOrWhiteSpace(request.BookingCode))
        {
            var trimCode = request.BookingCode.Trim();
            query = query.Where(b => b.QrCodeData == trimCode || b.BookingCode == trimCode);
        }
        else
        {
            return new StaffCheckOutResultDto { Succeeded = false, Message = "Vui lòng cung cấp BookingId hoặc BookingCode." };
        }

        var booking = await _unitOfWork.FirstOrDefaultAsync(query, cancellationToken);
        if (booking is null)
            return new StaffCheckOutResultDto { Succeeded = false, Message = "Không tìm thấy booking." };

        var isAssigned = await _staffUserService.IsStaffAssignedToPoolAsync(staffId, booking.PoolSlot.PoolId, cancellationToken);
        if (!isAssigned)
            return new StaffCheckOutResultDto { Succeeded = false, Message = "Bạn không có quyền quản lý tại hồ bơi này." };

        if (booking.Status != BookingStatus.CheckIn)
            return new StaffCheckOutResultDto
            {
                Succeeded = false,
                Message = $"Booking không thể check-out. Trạng thái hiện tại: {booking.Status} (chỉ cho phép check-out đối với booking ở trạng thái CheckIn)."
            };

        var localNow = DateTime.UtcNow.AddHours(7);
        var today = DateOnly.FromDateTime(localNow);

        if (booking.BookingDate != today)
            return new StaffCheckOutResultDto
            {
                Succeeded = false,
                Message = $"Không thể check-out booking ngày {booking.BookingDate:dd/MM/yyyy}. Hôm nay là {today:dd/MM/yyyy}."
            };

        var slotStart = booking.BookingDate.ToDateTime(TimeOnly.FromTimeSpan(booking.PoolSlot.StartTime));
        var slotEnd   = booking.BookingDate.ToDateTime(TimeOnly.FromTimeSpan(booking.PoolSlot.EndTime));

        if (localNow < slotStart)
            return new StaffCheckOutResultDto
            {
                Succeeded = false,
                Message = $"Ca bơi chưa bắt đầu. Check-out chỉ được thực hiện từ {slotStart:HH:mm} đến {slotEnd:HH:mm}."
            };

        if (localNow > slotEnd)
            return new StaffCheckOutResultDto
            {
                Succeeded = false,
                Message = $"Ca bơi đã kết thúc lúc {slotEnd:HH:mm}. Không thể check-out ngoài giờ slot."
            };

        booking.Status = BookingStatus.Completed;
        booking.UpdatedAt = DateTime.UtcNow;

        if (booking.CheckIn != null)
        {
            booking.CheckIn.CheckOutTime = DateTime.UtcNow;
        }

        _unitOfWork.Repository<Booking>().Update(booking);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new SlotCapacityFreedEvent
        {
            PoolSlotId = booking.PoolSlotId
        }, cancellationToken);

        var customer = await _staffUserService.GetUserBriefAsync(booking.UserId, cancellationToken);
        var customerName = customer?.FullName ?? "Khách vãng lai";
        var slotTime = $"{booking.PoolSlot.SlotName} ({booking.PoolSlot.StartTime:hh\\:mm} - {booking.PoolSlot.EndTime:hh\\:mm})";

        return new StaffCheckOutResultDto
        {
            Succeeded = true,
            Message = "Check-out thành công.",
            CustomerName = customerName,
            SlotTime = slotTime
        };
    }
}
