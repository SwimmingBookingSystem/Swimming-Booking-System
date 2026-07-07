using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.Customer_Bookings.Events;
using SBS.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Staff.Commands.CheckOut;

public record StaffCheckOutCommand : IRequest<StaffCheckOutResultDto>
{
    public int? BookingId { get; init; }
    public string? BookingCode { get; init; }
}

public record StaffCheckOutResultDto
{
    public bool Succeeded { get; init; }
    public string? Message { get; init; }
    public string? CustomerName { get; init; }
    public string? SlotTime { get; init; }
}

public class StaffCheckOutCommandHandler : IRequestHandler<StaffCheckOutCommand, StaffCheckOutResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IStaffUserService _staffUserService;
    private readonly IPublishEndpoint _publishEndpoint;

    public StaffCheckOutCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IStaffUserService staffUserService,
        IPublishEndpoint publishEndpoint)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _staffUserService = staffUserService;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<StaffCheckOutResultDto> Handle(StaffCheckOutCommand request, CancellationToken cancellationToken)
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

        if (booking.Status != "CheckIn")
            return new StaffCheckOutResultDto
            {
                Succeeded = false,
                Message = $"Booking không thể check-out. Trạng thái hiện tại: {booking.Status} (chỉ cho phép check-out đối với booking ở trạng thái CheckIn)."
            };

        booking.Status = "Completed";
        booking.UpdatedAt = DateTime.UtcNow;

        if (booking.CheckIn != null)
        {
            booking.CheckIn.CheckOutTime = DateTime.UtcNow;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish event to Waitlist processor
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
