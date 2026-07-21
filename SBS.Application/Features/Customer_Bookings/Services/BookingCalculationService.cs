using Microsoft.EntityFrameworkCore;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.Customer_Bookings.Dtos;
using SBS.Application.Features.Customer_Bookings.Interfaces;
using SBS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Customer_Bookings.Services;

public class BookingCalculationService : IBookingCalculationService
{
    private readonly IReadOnlyUnitOfWork _readOnlyUnitOfWork;

    public BookingCalculationService(IReadOnlyUnitOfWork readOnlyUnitOfWork)
    {
        _readOnlyUnitOfWork = readOnlyUnitOfWork;
    }

    public int CalculateSlotEquivalent(TicketType ticketType)
    {
        if (ticketType == null) return 1;

        if (string.Equals(ticketType.Category, "Combo", StringComparison.OrdinalIgnoreCase))
        {
            return ticketType.ComboItems != null && ticketType.ComboItems.Any()
                ? ticketType.ComboItems.Sum(c => c.Quantity)
                : 1;
        }

        return 1;
    }

    public async Task<int> GetBookedCapacityAsync(int poolSlotId, CancellationToken cancellationToken = default)
    {
        var currentBookedDetails = await _readOnlyUnitOfWork.Repository<BookingDetail>().Query()
            .AsNoTracking()
            .Include(bd => bd.PoolTicketType)
                .ThenInclude(pt => pt.TicketType)
                    .ThenInclude(tt => tt.ComboItems)
            .Where(bd => bd.Booking.PoolSlotId == poolSlotId && 
                         bd.Booking.Status != "Cancelled" && 
                         bd.Booking.Status != "Failed" && 
                         bd.Booking.Status != "Refunded")
            .ToListAsync(cancellationToken);

        int bookedCapacity = currentBookedDetails.Sum(bd => 
        {
            var tt = bd.PoolTicketType.TicketType;
            int slotEq = CalculateSlotEquivalent(tt);
            return bd.Quantity * slotEq;
        });

        return bookedCapacity;
    }

    public async Task<int> GetAvailableCapacityAsync(int poolSlotId, int totalCapacity, CancellationToken cancellationToken = default)
    {
        var bookedCapacity = await GetBookedCapacityAsync(poolSlotId, cancellationToken);
        return Math.Max(0, totalCapacity - bookedCapacity);
    }

    public int CalculateTotalRequestedSlots(IEnumerable<BookingTicketDto> requestedTickets, IEnumerable<PoolTicketType> ticketTypes)
    {
        int totalSlotsRequested = 0;
        var ticketTypeDict = ticketTypes.ToDictionary(t => t.PoolTicketTypeId, t => t.TicketType);

        foreach (var reqTicket in requestedTickets)
        {
            if (ticketTypeDict.TryGetValue(reqTicket.PoolTicketTypeId, out var ticketType))
            {
                int slotEq = CalculateSlotEquivalent(ticketType);
                totalSlotsRequested += reqTicket.Quantity * slotEq;
            }
        }

        return totalSlotsRequested;
    }

    public (decimal TotalAmount, List<BookingDetail> Details) CalculateBookingAmount(
        IEnumerable<BookingTicketDto> requestedTickets, 
        IEnumerable<PoolTicketType> ticketTypes)
    {
        decimal totalAmount = 0;
        var bookingDetails = new List<BookingDetail>();
        var ticketTypeDict = ticketTypes.ToDictionary(t => t.PoolTicketTypeId, t => t);

        foreach (var reqTicket in requestedTickets)
        {
            if (ticketTypeDict.TryGetValue(reqTicket.PoolTicketTypeId, out var ticketType))
            {
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
        }

        return (totalAmount, bookingDetails);
    }
}
