using MediatR;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.Customer_Bookings.Exceptions;
using SBS.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Customer_Bookings.Commands.CancelBooking;

public record CancelBookingCommand(int BookingId) : IRequest<bool>;

public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public CancelBookingCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _unitOfWork.Repository<Booking>().GetByIdAsync(request.BookingId);
        
        if (booking == null)
        {
            throw new BookingNotFoundException(request.BookingId);
        }

        if (booking.Status == "PendingPayment")
        {
            booking.Status = "Cancelled";
            _unitOfWork.Repository<Booking>().Update(booking);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }

        return false;
    }
}
