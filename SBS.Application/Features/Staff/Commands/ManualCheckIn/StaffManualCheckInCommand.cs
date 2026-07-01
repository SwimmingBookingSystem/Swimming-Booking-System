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
    public int BookingId { get; init; }
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
        var booking = await _unitOfWork.FirstOrDefaultAsync(
            bookingRepo.Query()
                .Include(b => b.CheckIn)
                .Include(b => b.PoolSlot)
                .Where(b => b.BookingId == request.BookingId),
            cancellationToken);

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

        var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
        if (booking.BookingDate != today)
            return new StaffCheckInResultDto
            {
                Succeeded = false,
                Message = $"Booking chỉ hợp lệ vào ngày {booking.BookingDate:dd/MM/yyyy}. Hôm nay là {today:dd/MM/yyyy}."
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
