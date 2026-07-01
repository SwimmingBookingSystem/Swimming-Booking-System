using MediatR;
using Microsoft.EntityFrameworkCore;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.Customer_Bookings.Dtos;
using SBS.Application.Features.Customer_Bookings.Exceptions;
using SBS.Application.Features.Customer_Bookings.Interfaces;
using SBS.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Customer_Bookings.Commands.CreateBooking;

public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, CreateBookingResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPoolSlotBookingRepository _poolSlotBookingRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPayOSService _payOSService;

    public CreateBookingCommandHandler(
        IUnitOfWork unitOfWork,
        IPoolSlotBookingRepository poolSlotBookingRepository,
        ICurrentUserService currentUserService,
        IPayOSService payOSService)
    {
        _unitOfWork = unitOfWork;
        _poolSlotBookingRepository = poolSlotBookingRepository;
        _currentUserService = currentUserService;
        _payOSService = payOSService;
    }

    public async Task<CreateBookingResponseDto> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        var totalQuantity = request.Tickets.Sum(t => t.Quantity);

        // Convert to Vietnam Timezone (+7) for accurate time checking
        var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        var vietnamTimeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
        var today = DateOnly.FromDateTime(vietnamTimeNow);
        var timeNow = vietnamTimeNow.TimeOfDay;

        var userIdString = _currentUserService.UserId;
        if (!Guid.TryParse(userIdString, out var userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        // Open transaction
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        
        try
        {
            // 1. Pessimistic Lock on PoolSlot
            var slot = await _poolSlotBookingRepository.GetPoolSlotWithLockAsync(request.PoolSlotId, cancellationToken);

            if (slot == null)
            {
                throw new SlotNotFoundException(request.PoolSlotId, today);
            }

            if (slot.Status != "Open")
            {
                throw new InvalidOperationException("Cannot book a slot that is not open.");
            }

            // 1.5 Check if the slot is in the past
            if (slot.SlotDate < today || (slot.SlotDate == today && slot.StartTime <= timeNow))
            {
                throw new InvalidOperationException("Cannot book a slot that has already started or passed.");
            }

            // 2. Check Capacity
            var currentBooked = await _unitOfWork.Repository<BookingDetail>().Query()
                .Where(bd => bd.Booking.PoolSlotId == slot.PoolSlotId && bd.Booking.Status != "Cancelled" && bd.Booking.Status != "Failed" && bd.Booking.Status != "Refunded")
                .SumAsync(bd => bd.Quantity, cancellationToken);

            if (slot.Capacity - currentBooked < totalQuantity)
            {
                throw new SlotFullException(slot.PoolSlotId, slot.SlotDate);
            }

            // 3. Retrieve requested ticket types and calculate total amount
            var poolTicketTypeIds = request.Tickets.Select(t => t.PoolTicketTypeId).ToList();
            var ticketTypes = await _unitOfWork.Repository<PoolTicketType>().Query()
                .Include(t => t.TicketType)
                .Where(t => poolTicketTypeIds.Contains(t.PoolTicketTypeId))
                .ToListAsync(cancellationToken);

            if (ticketTypes.Count != poolTicketTypeIds.Count)
            {
                throw new Exception("One or more invalid ticket types.");
            }

            if (ticketTypes.Any(t => t.Status != "Active" || t.TicketType.Status != "Active"))
            {
                throw new InvalidOperationException("One or more selected tickets are no longer active.");
            }

            // Cross-Pool Validation: Ensure all tickets belong to the same Pool as the slot
            if (ticketTypes.Any(t => t.PoolId != slot.PoolId))
            {
                throw new InvalidOperationException("One or more ticket types do not belong to the selected pool.");
            }

            decimal totalAmount = 0;
            var bookingDetails = new System.Collections.Generic.List<BookingDetail>();

            foreach (var reqTicket in request.Tickets)
            {
                var ticketType = ticketTypes.First(t => t.PoolTicketTypeId == reqTicket.PoolTicketTypeId);
                var actualPrice = ticketType.Price ?? Math.Round(ticketType.TicketType.BasePrice * (1 - ticketType.TicketType.DiscountPercent / 100m), 0);
                var subTotal = actualPrice * reqTicket.Quantity;
                totalAmount += subTotal;

                bookingDetails.Add(new BookingDetail
                {
                    PoolTicketTypeId = reqTicket.PoolTicketTypeId,
                    Quantity = reqTicket.Quantity,
                    UnitPrice = actualPrice,
                    SubTotal = subTotal
                });
            }

            // 4. Create Booking
            var booking = new Booking
            {
                BookingCode = $"BK-{DateTime.UtcNow:yyMMddHHmmss}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}",
                UserId = userId,
                PoolSlotId = slot.PoolSlotId,
                BookingDate = slot.SlotDate,
                Status = "PendingPayment",
                TotalAmount = totalAmount,
                BookingType = "Online",
                PaymentDeadline = DateTime.UtcNow.AddMinutes(15), // 15 mins to pay
                BookingDetails = bookingDetails
            };

            await _unitOfWork.Repository<Booking>().AddAsync(booking, cancellationToken);


            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 5. Generate Payment Link via PayOS
            var paymentLink = await _payOSService.CreatePaymentLinkAsync(booking.BookingId, booking.TotalAmount, booking.BookingCode, booking.PaymentDeadline.Value);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return new CreateBookingResponseDto
            {
                BookingId = booking.BookingId,
                BookingCode = booking.BookingCode,
                PaymentLink = paymentLink
            };
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
