using MediatR;
using Microsoft.EntityFrameworkCore;
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

        var userIdString = _currentUserService.UserId;
        if (!Guid.TryParse(userIdString, out var userId))
        {
            throw new UnauthorizedAccessException("Người dùng chưa được xác thực hoặc phiên đăng nhập đã hết hạn.");
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
                throw new InvalidOperationException("Không thể đặt suất bơi tại khung giờ đang bị đóng.");
            }

            // Booking remains open until the final 30 minutes of the swimming session.
            if (BookingTimePolicy.IsBookingClosed(slot.SlotDate, slot.EndTime, today, timeNow))
            {
                throw new InvalidOperationException("Không thể đặt vé khi ca bơi đã qua hoặc chỉ còn tối đa 30 phút.");
            }

            // 2. Retrieve requested ticket types
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

            // Cross-Pool Validation: Ensure all tickets belong to the same Pool as the slot
            if (ticketTypes.Any(t => t.PoolId != slot.PoolId))
            {
                throw new InvalidOperationException("Một hoặc nhiều loại vé đã chọn không thuộc về bể bơi này.");
            }

            // 3. Calculate requested slots using Domain Calculation Service
            int totalSlotsRequested = _bookingCalculationService.CalculateTotalRequestedSlots(request.Tickets, ticketTypes);

            if (totalSlotsRequested > 20)
            {
                throw new BadRequestException("Bạn chỉ được phép đặt tối đa 20 suất bơi trong một lần giao dịch.");
            }

            // 4. Calculate available capacity using Domain Calculation Service
            int availableCapacity = await _bookingCalculationService.GetAvailableCapacityAsync(slot.PoolSlotId, slot.Capacity, cancellationToken);

            if (availableCapacity < totalSlotsRequested)
            {
                throw new SlotFullException(slot.PoolSlotId, slot.SlotDate);
            }

            // 5. Calculate total amount & booking details
            var (totalAmount, bookingDetails) = _bookingCalculationService.CalculateBookingAmount(request.Tickets, ticketTypes);

            var bookingCutoffUtc = BookingTimePolicy.GetBookingCutoffUtc(slot.SlotDate, slot.EndTime);
            var paymentDeadline = utcNow.AddMinutes(15) < bookingCutoffUtc
                ? utcNow.AddMinutes(15)
                : bookingCutoffUtc;

            // 6. Create Booking
            var booking = new Booking
            {
                BookingCode = $"BK-{DateTime.UtcNow:yyMMddHHmmss}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}",
                UserId = userId,
                PoolSlotId = slot.PoolSlotId,
                BookingDate = slot.SlotDate,
                Status = "PendingPayment",
                TotalAmount = totalAmount,
                BookingType = "Online",
                PaymentDeadline = paymentDeadline,
                BookingDetails = bookingDetails
            };

            await _unitOfWork.Repository<Booking>().AddAsync(booking, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 7. Generate Payment Link via PayOS
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
