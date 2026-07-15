using MediatR;
using Microsoft.EntityFrameworkCore;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.Staff.Commands.QrCheckIn;
using SBS.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Staff.Commands.ManualCheckIn;

public record StaffManualCheckInCommand : IRequest<StaffCheckInResultDto>
{
    public int? BookingId { get; init; }
    public string? BookingCode { get; init; }
}

public class StaffManualCheckInCommandHandler : IRequestHandler<StaffManualCheckInCommand, StaffCheckInResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IStaffUserService _staffUserService;

    public StaffManualCheckInCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IStaffUserService staffUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _staffUserService = staffUserService;
    }

    public async Task<StaffCheckInResultDto> Handle(StaffManualCheckInCommand request, CancellationToken cancellationToken)
    {
        var staffIdString = _currentUserService.UserId;
        if (string.IsNullOrEmpty(staffIdString) || !Guid.TryParse(staffIdString, out var staffId))
            return new StaffCheckInResultDto { Succeeded = false, Message = "Nhân viên chưa đăng nhập hoặc không hợp lệ." };

        var bookingRepo = _unitOfWork.Repository<Booking>();
        var query = bookingRepo.Query()
            .Include(b => b.CheckIn)
            .Include(b => b.PoolSlot)
            .AsQueryable();

        // Tìm booking theo BookingId hoặc BookingCode (khớp với cách customer xem mã)
        if (request.BookingId.HasValue && request.BookingId.Value > 0)
        {
            query = query.Where(b => b.BookingId == request.BookingId.Value);
        }
        else if (!string.IsNullOrWhiteSpace(request.BookingCode))
        {
            var trimCode = request.BookingCode.Trim();
            // Tìm bằng BookingCode (e.g. BK-260714134512-AB3C) hoặc QrCodeData
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

        if (booking.Status != "Paid")
            return new StaffCheckInResultDto
            {
                Succeeded = false,
                Message = $"Booking không thể check-in. Trạng thái hiện tại: {booking.Status} (yêu cầu: Paid)."
            };

        // Kiểm tra ngày (giờ Việt Nam +7) — giống QR check-in
        var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
        if (booking.BookingDate != today)
            return new StaffCheckInResultDto
            {
                Succeeded = false,
                Message = $"Booking chỉ hợp lệ vào ngày {booking.BookingDate:dd/MM/yyyy}. Hôm nay là {today:dd/MM/yyyy}."
            };

        // Kiểm tra khung giờ hợp lệ (cho phép check-in trước 15 phút, và trước khi ca kết thúc) — giống QR check-in
        var localNow = DateTime.UtcNow.AddHours(7);
        var slotStartTime = TimeOnly.FromTimeSpan(booking.PoolSlot.StartTime);
        var slotEndTime = TimeOnly.FromTimeSpan(booking.PoolSlot.EndTime);
        var slotStartDateTime = booking.BookingDate.ToDateTime(slotStartTime);
        var slotEndDateTime = booking.BookingDate.ToDateTime(slotEndTime);
        var allowedCheckInStart = slotStartDateTime.AddMinutes(-15);

        if (localNow < allowedCheckInStart)
            return new StaffCheckInResultDto
            {
                Succeeded = false,
                Message = $"Ca bơi chưa bắt đầu. Bạn chỉ có thể check-in từ {allowedCheckInStart:HH:mm} (trước giờ bơi tối đa 15 phút)."
            };

        if (localNow > slotEndDateTime)
            return new StaffCheckInResultDto
            {
                Succeeded = false,
                Message = $"Ca bơi đã kết thúc vào lúc {slotEndTime:HH:mm}. Không thể check-in."
            };

        if (booking.CheckIn is not null)
            return new StaffCheckInResultDto
            {
                Succeeded = false,
                Message = $"Booking này đã được check-in lúc {booking.CheckIn.CheckInTime:HH:mm dd/MM/yyyy}."
            };

        var customerBrief = await _staffUserService.GetUserBriefAsync(booking.UserId, cancellationToken);

        var checkIn = new CheckIn
        {
            BookingId = booking.BookingId,
            CheckedByUserId = staffId,
            CheckInMethod = "Manual",
            CheckInTime = DateTime.UtcNow
        };
        await _unitOfWork.Repository<CheckIn>().AddAsync(checkIn, cancellationToken);

        booking.Status = "CheckIn";
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
}
