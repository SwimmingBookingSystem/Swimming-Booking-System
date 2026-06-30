using MediatR;
using Microsoft.EntityFrameworkCore;
using SBS.Application.Common.Interfaces;
using SBS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Staff.Commands.CreateWalkInBooking;

public record StaffCreateWalkInBookingCommand : IRequest<StaffWalkInBookingResultDto>
{
    /// <summary>Tên khách hàng walk-in (không cần có tài khoản)</summary>
    public string CustomerName { get; init; } = null!;

    /// <summary>Số điện thoại khách hàng</summary>
    public string CustomerPhone { get; init; } = null!;

    /// <summary>Email khách hàng (tùy chọn)</summary>
    public string? CustomerEmail { get; init; }

    /// <summary>ID slot đã chọn</summary>
    public int PoolSlotId { get; init; }

    /// <summary>Ngày bơi</summary>
    public DateOnly BookingDate { get; init; }

    /// <summary>Danh sách loại vé và số lượng</summary>
    public List<StaffTicketOrderItemDto> Tickets { get; init; } = new();

    /// <summary>Xác nhận nhân viên đã thu tiền mặt</summary>
    public bool CashPaymentConfirmed { get; init; }
}

public record StaffTicketOrderItemDto
{
    public int PoolTicketTypeId { get; init; }
    public int Quantity { get; init; }
}

public record StaffWalkInBookingResultDto
{
    public bool Succeeded { get; init; }
    public string? BookingCode { get; init; }
    public int? BookingId { get; init; }
    public decimal? TotalAmount { get; init; }
    public string[]? Errors { get; init; }
}

public class StaffCreateWalkInBookingCommandHandler : IRequestHandler<StaffCreateWalkInBookingCommand, StaffWalkInBookingResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public StaffCreateWalkInBookingCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<StaffWalkInBookingResultDto> Handle(StaffCreateWalkInBookingCommand request, CancellationToken cancellationToken)
    {
        // 1. Lấy StaffId
        var staffIdString = _currentUserService.UserId;
        if (string.IsNullOrEmpty(staffIdString) || !Guid.TryParse(staffIdString, out var staffId))
            return new StaffWalkInBookingResultDto { Succeeded = false, Errors = new[] { "Nhân viên chưa đăng nhập." } };

        // 2. Kiểm tra slot tồn tại và còn mở
        var slot = await _unitOfWork.FirstOrDefaultAsync(
            _unitOfWork.Repository<PoolSlot>()
                .Query()
                .Include(s => s.Pool)
                .Where(s => s.PoolSlotId == request.PoolSlotId),
            cancellationToken);

        if (slot is null)
            return new StaffWalkInBookingResultDto { Succeeded = false, Errors = new[] { "Không tìm thấy slot bơi." } };

        if (slot.Status != "Open")
            return new StaffWalkInBookingResultDto { Succeeded = false, Errors = new[] { $"Slot bơi hiện không mở (Trạng thái: {slot.Status})." } };

        // Guard: kiểm tra Staff có được phân công vào hồ bơi của slot này không
        var isAssigned = await _unitOfWork.AnyAsync(
            _unitOfWork.Repository<PoolStaffAssignment>().Query()
                .Where(a => a.StaffId == staffId && a.PoolId == slot.PoolId),
            cancellationToken);
        if (!isAssigned)
            return new StaffWalkInBookingResultDto { Succeeded = false, Errors = new[] { "Bạn không có quyền tạo walk-in booking tại hồ bơi này." } };

        // 3. Kiểm tra sức chứa còn lại
        var confirmedCount = await _unitOfWork.CountAsync(
            _unitOfWork.Repository<Booking>()
                .Query()
                .Where(b => b.PoolSlotId == request.PoolSlotId
                         && b.BookingDate == request.BookingDate
                         && (b.Status == "Confirmed" || b.Status == "CheckIn")),
            cancellationToken);

        if (confirmedCount >= slot.Capacity)
            return new StaffWalkInBookingResultDto { Succeeded = false, Errors = new[] { "Slot đã đầy chỗ." } };

        // 4. Lấy thông tin vé và tính tổng tiền
        var poolTicketTypeIds = request.Tickets.Select(t => t.PoolTicketTypeId).ToList();
        var poolTicketTypes = await _unitOfWork.ToListAsync(
            _unitOfWork.Repository<PoolTicketType>()
                .Query()
                .Include(pt => pt.TicketType)
                .Where(pt => poolTicketTypeIds.Contains(pt.PoolTicketTypeId)
                          && pt.PoolId == slot.PoolId
                          && pt.Status == "Active"),
            cancellationToken);

        if (poolTicketTypes.Count != request.Tickets.Count)
            return new StaffWalkInBookingResultDto { Succeeded = false, Errors = new[] { "Một hoặc nhiều loại vé không hợp lệ hoặc không thuộc bể bơi này." } };

        decimal totalAmount = 0;
        var bookingDetails = new List<BookingDetail>();

        foreach (var ticketOrder in request.Tickets)
        {
            var ptt = poolTicketTypes.First(pt => pt.PoolTicketTypeId == ticketOrder.PoolTicketTypeId);
            var actualPrice = ptt.Price ?? Math.Round(ptt.TicketType.BasePrice * (1 - ptt.TicketType.DiscountPercent / 100m), 0);
            var subTotal = actualPrice * ticketOrder.Quantity;
            totalAmount += subTotal;

            bookingDetails.Add(new BookingDetail
            {
                PoolTicketTypeId = ptt.PoolTicketTypeId,
                Quantity = ticketOrder.Quantity,
                UnitPrice = actualPrice,
                SubTotal = subTotal
            });
        }

        // 5. Generate BookingCode: SBS-YYYYMMDD-XXXXXX
        var bookingCode = $"SBS-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

        // 6. Tạo Booking
        var booking = new Booking
        {
            BookingCode = bookingCode,
            UserId = staffId, // Walk-in: gán staffId tạm thời làm đại diện
            PoolSlotId = request.PoolSlotId,
            BookingDate = request.BookingDate,
            Status = "Confirmed",           // Walk-in = đã xác nhận ngay
            TotalAmount = totalAmount,
            BookingType = "WalkIn",
            PaymentDeadline = null,         // Walk-in không có deadline thanh toán
            CreatedAt = DateTime.UtcNow,
            BookingDetails = bookingDetails
        };

        await _unitOfWork.Repository<Booking>().AddAsync(booking, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken); // Lưu để lấy BookingId

        // 7. Tạo Payment Cash
        var payment = new Payment
        {
            BookingId = booking.BookingId,
            PaymentMethod = "Cash",
            Amount = totalAmount,
            Status = "Completed",
            PaymentDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.Repository<Payment>().AddAsync(payment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new StaffWalkInBookingResultDto
        {
            Succeeded = true,
            BookingCode = bookingCode,
            BookingId = booking.BookingId,
            TotalAmount = totalAmount
        };
    }
}
