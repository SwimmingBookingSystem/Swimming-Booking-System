using MediatR;
using Microsoft.EntityFrameworkCore;
using SBS.Application.Common.Dtos.Staff;
using SBS.Application.Common.Interfaces;
using SBS.Domain.Entities;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Staff.Queries.GetBookingDetail;

public record StaffGetBookingDetailQuery : IRequest<BookingDetailDto?>
{
    public int BookingId { get; init; }
}

public class StaffGetBookingDetailQueryHandler : IRequestHandler<StaffGetBookingDetailQuery, BookingDetailDto?>
{
    private readonly IReadOnlyUnitOfWork _readOnlyUnitOfWork;
    private readonly IStaffUserService _staffUserService;

    public StaffGetBookingDetailQueryHandler(
        IReadOnlyUnitOfWork readOnlyUnitOfWork,
        IStaffUserService staffUserService)
    {
        _readOnlyUnitOfWork = readOnlyUnitOfWork;
        _staffUserService = staffUserService;
    }

    public async Task<BookingDetailDto?> Handle(StaffGetBookingDetailQuery request, CancellationToken cancellationToken)
    {
        var booking = await _readOnlyUnitOfWork.FirstOrDefaultAsync(
            _readOnlyUnitOfWork.Repository<Booking>()
                .Query()
                .Include(b => b.PoolSlot)
                    .ThenInclude(s => s.Pool)
                .Include(b => b.BookingDetails)
                    .ThenInclude(d => d.PoolTicketType)
                        .ThenInclude(pt => pt.TicketType)
                .Include(b => b.Payment)
                .Include(b => b.CheckIn)
                .Where(b => b.BookingId == request.BookingId),
            cancellationToken);

        if (booking is null)
            return null;

        // Enrich thông tin khách hàng từ IStaffUserService
        var customer = await _staffUserService.GetUserBriefAsync(booking.UserId, cancellationToken);

        return new BookingDetailDto
        {
            BookingId = booking.BookingId,
            BookingCode = booking.BookingCode,
            Status = booking.Status,
            BookingDate = booking.BookingDate,
            TotalAmount = booking.TotalAmount,
            BookingType = booking.BookingType,
            PaymentDeadline = booking.PaymentDeadline,
            CreatedAt = booking.CreatedAt,

            CustomerId = booking.UserId,
            CustomerName = customer?.FullName ?? "Khách vãng lai",
            CustomerEmail = customer?.Email ?? string.Empty,
            CustomerPhone = customer?.PhoneNumber,

            PoolName = booking.PoolSlot.Pool.PoolName,
            SlotTime = $"{booking.PoolSlot.StartTime:hh\\:mm} - {booking.PoolSlot.EndTime:hh\\:mm}",

            Tickets = booking.BookingDetails.Select(d => new BookingTicketItemDto
            {
                TicketName = d.PoolTicketType.TicketType.TicketName,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                SubTotal = d.SubTotal
            }).ToList(),

            Payment = booking.Payment is null ? null : new PaymentInfoDto
            {
                PaymentMethod = booking.Payment.PaymentMethod,
                Status = booking.Payment.Status,
                Amount = booking.Payment.Amount,
                PaymentDate = booking.Payment.PaymentDate
            },

            CheckIn = booking.CheckIn is null ? null : new CheckInInfoDto
            {
                CheckInMethod = booking.CheckIn.CheckInMethod,
                CheckInTime = booking.CheckIn.CheckInTime
            }
        };
    }
}
