using MediatR;
using Microsoft.EntityFrameworkCore;
using SBS.Application.Common;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.Customer_Bookings.Dtos;
using SBS.Application.Features.Customer_Bookings.Interfaces;
using SBS.Application.Features.Customer_Bookings.Policies;
using SBS.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Customer_Bookings.Commands.JoinWaitlist;

public class JoinWaitlistCommandHandler : IRequestHandler<JoinWaitlistCommand, JoinWaitlistResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPoolSlotBookingRepository _poolSlotBookingRepository;
    private readonly IBookingCalculationService _bookingCalculationService;
    private readonly ICurrentUserService _currentUserService;

    public JoinWaitlistCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IPoolSlotBookingRepository poolSlotBookingRepository,
        IBookingCalculationService bookingCalculationService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _poolSlotBookingRepository = poolSlotBookingRepository;
        _bookingCalculationService = bookingCalculationService;
    }

    public async Task<JoinWaitlistResultDto> Handle(JoinWaitlistCommand request, CancellationToken cancellationToken)
    {
        var userIdStr = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            return new JoinWaitlistResultDto { Succeeded = false, Message = "Vui lòng đăng nhập để tham gia hàng đợi." };
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        var committed = false;
        try
        {
        var slot = await _poolSlotBookingRepository
            .GetPoolSlotWithLockAsync(request.PoolSlotId, cancellationToken);

        if (slot == null)
            return new JoinWaitlistResultDto { Succeeded = false, Message = "Không tìm thấy ca bơi." };

        if (slot.Status != "Open")
            return new JoinWaitlistResultDto { Succeeded = false, Message = "Ca bơi này hiện không mở để tham gia hàng đợi." };

        var (today, timeNow) = BookingTimePolicy.GetVietnamDateAndTime(DateTime.Now);
        if (BookingTimePolicy.IsBookingClosed(slot.SlotDate, slot.EndTime, today, timeNow))
        {
            return new JoinWaitlistResultDto
            {
                Succeeded = false,
                Message = "Không thể tham gia hàng đợi khi ca bơi đã qua hoặc chỉ còn tối đa 30 phút."
            };
        }

        var availableCapacity = await _bookingCalculationService
            .GetAvailableCapacityAsync(slot.PoolSlotId, slot.Capacity, cancellationToken);
        if (availableCapacity > 0)
        {
            return new JoinWaitlistResultDto { Succeeded = false, Message = "Ca bơi này vẫn còn chỗ trống, bạn có thể đặt lịch trực tiếp." };
        }

        // Check if user is already in waitlist for this slot
        var waitlistRepo = _unitOfWork.Repository<WaitlistEntry>();
        var alreadyInWaitlist = await waitlistRepo.Query()
            .AnyAsync(w => w.PoolSlotId == request.PoolSlotId && w.UserId == userId &&
                           (w.Status == WaitlistStatus.Waiting || w.Status == WaitlistStatus.Offered),
                cancellationToken);

        if (alreadyInWaitlist)
            return new JoinWaitlistResultDto { Succeeded = false, Message = "Bạn đã có một lượt đang hoạt động trong hàng chờ của ca bơi này." };

        var maxPosition = await waitlistRepo.Query()
            .Where(w => w.PoolSlotId == request.PoolSlotId)
            .MaxAsync(w => (int?)w.Position, cancellationToken) ?? 0;

        var newEntry = new WaitlistEntry
        {
            UserId = userId,
            PoolSlotId = request.PoolSlotId,
            Quantity = 1,
            Position = maxPosition + 1,
            Status = WaitlistStatus.Waiting,
            CreatedAt = DateTime.Now
        };

        await waitlistRepo.AddAsync(newEntry, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
        committed = true;

        return new JoinWaitlistResultDto
        {
            Succeeded = true,
            Message = $"Bạn đã tham gia hàng chờ thành công ở vị trí số {newEntry.Position}. " +
                      "Mỗi lượt hàng chờ tương ứng với 1 vé đơn; hệ thống sẽ gửi email khi đến lượt thanh toán."
        };
        }
        finally
        {
            if (!committed)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            }
        }
    }
}
