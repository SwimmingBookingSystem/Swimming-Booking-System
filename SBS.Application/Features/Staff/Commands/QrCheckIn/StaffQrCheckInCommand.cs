using MediatR;
using Microsoft.EntityFrameworkCore;
using SBS.Application.Common.Interfaces;
using SBS.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Staff.Commands.QrCheckIn;

public record StaffQrCheckInCommand : IRequest<StaffCheckInResultDto>
{
    /// <summary>Mã BookingCode đọc từ QR code.</summary>
    public string BookingCode { get; init; } = null!;
}

public record StaffCheckInResultDto
{
    public bool Succeeded { get; init; }
    public string? Message { get; init; }
    public string? CustomerName { get; init; }
    public string? SlotTime { get; init; }
}

public class StaffQrCheckInCommandHandler : IRequestHandler<StaffQrCheckInCommand, StaffCheckInResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IStaffUserService _staffUserService;

    public StaffQrCheckInCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IStaffUserService staffUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _staffUserService = staffUserService;
    }

    public async Task<StaffCheckInResultDto> Handle(StaffQrCheckInCommand request, CancellationToken cancellationToken)
    {
        // 1. Lấy StaffId từ context
        var staffIdString = _currentUserService.UserId;
        if (string.IsNullOrEmpty(staffIdString) || !Guid.TryParse(staffIdString, out var staffId))
            return new StaffCheckInResultDto { Succeeded = false, Message = "Nhân viên chưa đăng nhập hoặc không hợp lệ." };

        // 2. Tìm booking theo BookingCode
        var bookingRepo = _unitOfWork.Repository<Booking>();
        var booking = await _unitOfWork.FirstOrDefaultAsync(
            bookingRepo.Query()
                .Include(b => b.CheckIn)
                .Include(b => b.PoolSlot)
                .Where(b => b.BookingCode == request.BookingCode),
            cancellationToken);

        if (booking is null)
            return new StaffCheckInResultDto { Succeeded = false, Message = "Không tìm thấy booking với mã QR này." };

        // 3. Validate trạng thái
        if (booking.Status != "Confirmed")
            return new StaffCheckInResultDto
            {
                Succeeded = false,
                Message = $"Booking không thể check-in. Trạng thái hiện tại: {booking.Status}."
            };

        // 4. Validate ngày
        var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7)); // UTC+7
        if (booking.BookingDate != today)
            return new StaffCheckInResultDto
            {
                Succeeded = false,
                Message = $"Booking chỉ hợp lệ vào ngày {booking.BookingDate:dd/MM/yyyy}. Hôm nay là {today:dd/MM/yyyy}."
            };

        // 5. Kiểm tra đã check-in chưa
        if (booking.CheckIn is not null)
            return new StaffCheckInResultDto
            {
                Succeeded = false,
                Message = $"Booking này đã được check-in lúc {booking.CheckIn.CheckInTime:HH:mm dd/MM/yyyy}."
            };

        // 6. Lấy thông tin khách hàng
        var customerBrief = await _staffUserService.GetUserBriefAsync(booking.UserId, cancellationToken);

        // 7. Tạo CheckIn record
        var checkIn = new CheckIn
        {
            BookingId = booking.BookingId,
            CheckedByUserId = staffId,
            CheckInMethod = "QR",
            CheckInTime = DateTime.UtcNow
        };
        await _unitOfWork.Repository<CheckIn>().AddAsync(checkIn, cancellationToken);

        // 8. Cập nhật trạng thái booking
        booking.Status = "CheckIn";
        booking.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Repository<Booking>().Update(booking);

        // 9. Lưu
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
}
