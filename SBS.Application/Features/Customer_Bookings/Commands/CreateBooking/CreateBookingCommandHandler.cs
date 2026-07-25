using MediatR;
using Microsoft.EntityFrameworkCore;
using SBS.Application.Common;
using SBS.Application.Common.Interfaces;
using SBS.Application.Common.ManagerExceptions;
using SBS.Application.Features.Customer_Bookings.Dtos;
using SBS.Application.Features.Customer_Bookings.Exceptions;
using SBS.Application.Features.Customer_Bookings.Interfaces;
using SBS.Application.Features.Customer_Bookings.Policies;
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
    private readonly IBookingCalculationService _bookingCalculationService;

    public CreateBookingCommandHandler(
        IUnitOfWork unitOfWork,
        IPoolSlotBookingRepository poolSlotBookingRepository,
        ICurrentUserService currentUserService,
        IPayOSService payOSService,
        IBookingCalculationService bookingCalculationService)
    {
        _unitOfWork = unitOfWork;
        _poolSlotBookingRepository = poolSlotBookingRepository;
        _currentUserService = currentUserService;
        _payOSService = payOSService;
        _bookingCalculationService = bookingCalculationService;
    }

    public async Task<CreateBookingResponseDto> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;
        var (today, timeNow) = BookingTimePolicy.GetVietnamDateAndTime(utcNow);

        if (!Guid.TryParse(_currentUserService.UserId, out var userId))
        {
            throw new UnauthorizedAccessException("Người dùng chưa được xác thực hoặc phiên đăng nhập đã hết hạn.");
        }

        Booking booking;
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var slot = await _poolSlotBookingRepository.GetPoolSlotWithLockAsync(request.PoolSlotId, cancellationToken)
                ?? throw new SlotNotFoundException(request.PoolSlotId, today);

            if (slot.Status != "Open")
            {
                throw new InvalidOperationException("Không thể đặt suất bơi tại khung giờ đang bị đóng.");
            }

            if (BookingTimePolicy.IsBookingClosed(slot.SlotDate, slot.EndTime, today, timeNow))
            {
                throw new InvalidOperationException("Không thể đặt vé khi ca bơi đã qua hoặc chỉ còn tối đa 30 phút.");
            }

            var poolTicketTypeIds = request.Tickets.Select(t => t.PoolTicketTypeId).ToList();
            var ticketTypes = await _unitOfWork.Repository<PoolTicketType>().Query()
                .Include(t => t.TicketType)
                    .ThenInclude(tt => tt.ComboItems)
                .Where(t => poolTicketTypeIds.Contains(t.PoolTicketTypeId))
                .ToListAsync(cancellationToken);

            if (ticketTypes.Count != poolTicketTypeIds.Count)
            {
                throw new BadRequestException("Một hoặc nhiều loại vé không hợp lệ hoặc không tồn tại.");
            }

            if (ticketTypes.Any(t => t.Status != "Active" || t.TicketType.Status != "Active"))
            {
                throw new BadRequestException("Rất tiếc, một số loại vé bạn chọn vừa được hệ thống ngừng kinh doanh.");
            }

            if (ticketTypes.Any(t => t.PoolId != slot.PoolId))
            {
                throw new InvalidOperationException("Một hoặc nhiều loại vé đã chọn không thuộc về bể bơi này.");
            }

            var totalSlotsRequested = _bookingCalculationService.CalculateTotalRequestedSlots(request.Tickets, ticketTypes);
            if (totalSlotsRequested > 20)
            {
                throw new BadRequestException("Bạn chỉ được phép đặt tối đa 20 suất bơi trong một lần giao dịch.");
            }

            var availableCapacity = await _bookingCalculationService.GetAvailableCapacityAsync(
                slot.PoolSlotId, slot.Capacity, cancellationToken);
            if (availableCapacity < totalSlotsRequested)
            {
                throw new SlotFullException(slot.PoolSlotId, slot.SlotDate);
            }

            var (totalAmount, bookingDetails) = _bookingCalculationService.CalculateBookingAmount(request.Tickets, ticketTypes);
            var bookingCutoffUtc = BookingTimePolicy.GetBookingCutoffUtc(slot.SlotDate, slot.EndTime);
            var paymentDeadline = utcNow.AddMinutes(15) < bookingCutoffUtc
                ? utcNow.AddMinutes(15)
                : bookingCutoffUtc;

            booking = new Booking
            {
                BookingCode = $"BK-{utcNow:yyMMddHHmmss}-{Guid.NewGuid().ToString()[..4].ToUpperInvariant()}",
                UserId = userId,
                PoolSlotId = slot.PoolSlotId,
                BookingDate = slot.SlotDate,
                Status = BookingStatus.PendingPayment,
                TotalAmount = totalAmount,
                BookingType = "Online",
                PaymentDeadline = paymentDeadline,
                BookingDetails = bookingDetails
            };

            await _unitOfWork.Repository<Booking>().AddAsync(booking, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        try
        {
            var paymentLink = await _payOSService.CreatePaymentLinkAsync(
                booking.BookingId,
                booking.TotalAmount,
                booking.BookingCode,
                booking.PaymentDeadline!.Value);

            return new CreateBookingResponseDto
            {
                BookingId = booking.BookingId,
                BookingCode = booking.BookingCode,
                PaymentLink = paymentLink
            };
        }
        catch
        {
            // The booking is already durable. Keep it pending so a successful but unacknowledged
            // PayOS request can still be reconciled; the expiration worker will release its capacity.
            throw;
        }
    }

}
