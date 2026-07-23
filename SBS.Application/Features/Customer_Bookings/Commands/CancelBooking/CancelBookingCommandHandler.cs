using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SBS.Application.Common;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.Customer_Bookings.Events;
using SBS.Application.Features.Customer_Bookings.Exceptions;
using SBS.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Customer_Bookings.Commands.CancelBooking;

public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;

    public CancelBookingCommandHandler(IUnitOfWork unitOfWork, IPublishEndpoint publishEndpoint)
    {
        _unitOfWork = unitOfWork;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<bool> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _unitOfWork.Repository<Booking>().GetByIdAsync(request.BookingId);
        
        if (booking == null)
        {
            throw new BookingNotFoundException(request.BookingId);
        }

        if (booking.Status != BookingStatus.PendingPayment)
        {
            throw new System.InvalidOperationException("Chỉ có thể hủy vé khi ở trạng thái Chờ thanh toán.");
        }

        booking.Status = BookingStatus.Cancelled;
        _unitOfWork.Repository<Booking>().Update(booking);

        // Nếu booking này thuộc về một Waitlist (do Consumer tạo ra), đánh dấu Waitlist là Cancelled
        var waitlistEntry = await _unitOfWork.Repository<WaitlistEntry>().Query()
            .FirstOrDefaultAsync(w => w.BookingId == booking.BookingId && w.Status == WaitlistStatus.Offered, cancellationToken);
        if (waitlistEntry != null)
        {
            waitlistEntry.Status = WaitlistStatus.Cancelled;
            _unitOfWork.Repository<WaitlistEntry>().Update(waitlistEntry);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Bắn event để Waitlist xử lý (kêu gọi người khác vào)
        await _publishEndpoint.Publish(new SlotCapacityFreedEvent
        {
            PoolSlotId = booking.PoolSlotId
        }, cancellationToken);

        return true;
    }
}
