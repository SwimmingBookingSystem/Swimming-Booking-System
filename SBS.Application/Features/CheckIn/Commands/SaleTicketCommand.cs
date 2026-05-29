using MediatR;
using Microsoft.EntityFrameworkCore;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.CheckIn.DTOs;
using SBS.Domain.Entities;

namespace SBS.Application.Features.CheckIn.Commands;

// ── COMMAND ───────────────────────────────────────────────────────────────────

public record SaleTicketCommand(SaleTicketRequestDto Request) : IRequest<SaleTicketResponseDto>;

// ── HANDLER ───────────────────────────────────────────────────────────────────

public sealed class SaleTicketCommandHandler : IRequestHandler<SaleTicketCommand, SaleTicketResponseDto>
{
    private readonly IApplicationDbContext _context;

    public SaleTicketCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SaleTicketResponseDto> Handle(SaleTicketCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        // 1. Verify Pool exists
        var pool = await _context.Pools
            .FirstOrDefaultAsync(p => p.PoolId == request.PoolId, cancellationToken)
            ?? throw new InvalidOperationException($"Không tìm thấy bể bơi với ID {request.PoolId}.");

        if (pool.PoolStatus == false)
            throw new InvalidOperationException("Bể bơi này hiện đang đóng cửa hoặc tạm dừng hoạt động.");

        // 2. Verify TicketType exists and is active at this pool
        var poolTicketType = await _context.PoolTicketTypes
            .Include(pt => pt.TicketType)
            .FirstOrDefaultAsync(pt => pt.PoolId == request.PoolId && pt.TicketTypeId == request.TicketTypeId && pt.Status == "active", cancellationToken)
            ?? throw new InvalidOperationException($"Vé loại ID {request.TicketTypeId} không hoạt động hoặc không được áp dụng tại bể bơi này.");

        var ticketType = poolTicketType.TicketType;

        // 3. Verify Pool capacity (Slot checking)
        var bookedSlots = await _context.Bookings
            .Where(b => b.PoolId == request.PoolId && b.BookingDate == request.BookingDate && b.BookingStatus != "cancelled")
            .Where(b => b.StartTime < request.EndTime && b.EndTime > request.StartTime)
            .SumAsync(b => b.SlotCount, cancellationToken);

        if (bookedSlots + request.SlotCount > pool.MaxSlot)
        {
            throw new InvalidOperationException($"Bể bơi đã hết chỗ trong khung giờ này. Còn lại {pool.MaxSlot - bookedSlots} chỗ trống.");
        }

        // 4. Calculate prices & discounts
        decimal itemPrice = ticketType.BasePrice;
        if (ticketType.DiscountPercent.HasValue && ticketType.DiscountPercent.Value > 0)
        {
            var tp = ticketType.DiscountPercent.Value;
            var tpd = tp > 1 ? tp / 100 : tp;
            itemPrice = itemPrice * (1 - tpd);
        }

        decimal subtotal = itemPrice * request.SlotCount;
        decimal totalAmount = subtotal;
        decimal discountAmount = 0;

        Discount? discount = null;
        if (request.DiscountId.HasValue)
        {
            discount = await _context.Discounts
                .FirstOrDefaultAsync(d => d.DiscountId == request.DiscountId.Value, cancellationToken)
                ?? throw new InvalidOperationException($"Mã giảm giá với ID {request.DiscountId} không tồn tại.");

            if (discount.Status == false || DateTime.UtcNow < discount.ValidFrom || DateTime.UtcNow > discount.ValidTo)
                throw new InvalidOperationException("Mã giảm giá này đã hết hạn hoặc chưa đến thời gian áp dụng.");

            if (discount.Quantity.HasValue)
            {
                if (discount.Quantity.Value <= 0)
                    throw new InvalidOperationException("Mã giảm giá đã hết lượt sử dụng.");
                
                discount.Quantity--;
            }

            var dp = discount.DiscountPercent;
            var dpd = dp > 1 ? dp / 100 : dp;
            totalAmount = subtotal * (1 - dpd);
            discountAmount = subtotal - totalAmount;
        }

        // 5. Create Booking
        var booking = new Booking
        {
            UserId = request.UserId,
            PoolId = request.PoolId,
            DiscountId = request.DiscountId,
            BookingDate = request.BookingDate,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            SlotCount = request.SlotCount,
            BookingStatus = "paid", // Direct sales at the counter are paid immediately
            CreatedAt = DateTime.UtcNow
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync(cancellationToken); // Save to get BookingId

        // 6. Generate Ticket Code & Create Ticket
        string ticketCode = "TICK-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
        var ticket = new Ticket
        {
            BookingId = booking.BookingId,
            TicketTypeId = request.TicketTypeId,
            TicketPrice = itemPrice,
            TicketCode = ticketCode,
            IssuedBy = request.StaffId,
            IssuedAt = DateTime.UtcNow
        };

        _context.Tickets.Add(ticket);

        // 7. Create Payment record
        var payment = new Payment
        {
            BookingId = booking.BookingId,
            PaymentMethod = request.PaymentMethod,
            PaymentStatus = "paid",
            PaymentDate = DateTime.UtcNow,
            TotalAmount = totalAmount,
            DiscountAmount = discountAmount,
            TransactionReference = "POS-" + Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper(),
            CreatedAt = DateTime.UtcNow
        };

        _context.Payments.Add(payment);

        // 8. Create SaleTicketDirectly record
        var saleDirectly = new SaleTicketDirectly
        {
            CustomerName = request.CustomerName ?? (request.UserId.HasValue ? "Khách hàng thành viên" : "Khách vãng lai"),
            CustomerPhone = request.CustomerPhone,
            CustomerEmail = request.CustomerEmail,
            UserId = request.UserId,
            StaffId = request.StaffId,
            BookingId = booking.BookingId,
            TotalAmount = totalAmount,
            PaymentMethod = request.PaymentMethod,
            PaymentStatus = "completed",
            Notes = request.Notes,
            SaleDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.SaleTicketDirectlys.Add(saleDirectly);

        // Save everything
        await _context.SaveChangesAsync(cancellationToken);

        return new SaleTicketResponseDto
        {
            SaleId = saleDirectly.SaleId,
            BookingId = booking.BookingId,
            TicketCode = ticketCode,
            TotalAmount = totalAmount,
            PaymentMethod = request.PaymentMethod,
            PaymentStatus = "completed",
            SaleDate = saleDirectly.SaleDate,
            CustomerName = saleDirectly.CustomerName,
            StaffId = request.StaffId,
            Message = "Bán vé trực tiếp và thanh toán thành công!"
        };
    }
}
