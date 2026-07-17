using MediatR;
using Microsoft.EntityFrameworkCore;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.Customer_Bookings.Dtos;
using SBS.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Customer_Bookings.Commands.JoinWaitlist;

public class JoinWaitlistCommandHandler : IRequestHandler<JoinWaitlistCommand, JoinWaitlistResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public JoinWaitlistCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<JoinWaitlistResultDto> Handle(JoinWaitlistCommand request, CancellationToken cancellationToken)
    {
        var userIdStr = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            return new JoinWaitlistResultDto { Succeeded = false, Message = "Vui lòng đăng nhập để tham gia hàng đợi." };
        }

        var poolSlotRepo = _unitOfWork.Repository<PoolSlot>();
        var slot = await poolSlotRepo.Query()
            .FirstOrDefaultAsync(s => s.PoolSlotId == request.PoolSlotId, cancellationToken);

        if (slot == null)
            return new JoinWaitlistResultDto { Succeeded = false, Message = "Không tìm thấy ca bơi." };

        var currentBookedDetails = await _unitOfWork.Repository<BookingDetail>().Query()
            .Include(bd => bd.PoolTicketType)
                .ThenInclude(pt => pt.TicketType)
                    .ThenInclude(tt => tt.ComboItems)
            .Where(bd => bd.Booking.PoolSlotId == request.PoolSlotId && 
                         bd.Booking.Status != "Cancelled" && 
                         bd.Booking.Status != "Failed" &&
                         bd.Booking.Status != "Refunded")
            .ToListAsync(cancellationToken);

        int bookedCapacity = currentBookedDetails.Sum(bd => 
        {
            var tt = bd.PoolTicketType.TicketType;
            int slotEq = tt.Category == "Combo" ? tt.ComboItems.Sum(c => c.Quantity) : 1;
            return bd.Quantity * slotEq;
        });

        var availableCapacity = slot.Capacity - bookedCapacity;
        if (availableCapacity > 0)
        {
            return new JoinWaitlistResultDto { Succeeded = false, Message = "Ca bơi này vẫn còn chỗ trống, bạn có thể đặt lịch trực tiếp." };
        }

        // Check if user is already in waitlist for this slot
        var waitlistRepo = _unitOfWork.Repository<WaitlistEntry>();
        var alreadyInWaitlist = await waitlistRepo.Query()
            .AnyAsync(w => w.PoolSlotId == request.PoolSlotId && w.UserId == userId && w.Status == "Waiting", cancellationToken);

        if (alreadyInWaitlist)
            return new JoinWaitlistResultDto { Succeeded = false, Message = "Bạn đã tham gia hàng đợi cho ca bơi này rồi." };

        var maxPosition = await waitlistRepo.Query()
            .Where(w => w.PoolSlotId == request.PoolSlotId)
            .MaxAsync(w => (int?)w.Position, cancellationToken) ?? 0;

        var newEntry = new WaitlistEntry
        {
            UserId = userId,
            PoolSlotId = request.PoolSlotId,
            Quantity = request.Quantity,
            Position = maxPosition + 1,
            Status = "Waiting",
            CreatedAt = DateTime.UtcNow
        };

        await waitlistRepo.AddAsync(newEntry, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new JoinWaitlistResultDto
        {
            Succeeded = true,
            Message = "Đăng ký tham gia hàng đợi thành công! Chúng tôi sẽ email cho bạn nếu có vé trống."
        };
    }
}
