using System;

namespace SBS.Domain.Entities;

public class BookingDetail : BaseEntity
{
    public Guid BookingId { get; private set; }
    public Guid ScheduleId { get; private set; }
    public DateTime TicketDate { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal SubTotal { get; private set; }

    // Navigation properties
    public virtual Booking Booking { get; private set; }
    public virtual PoolSchedule Schedule { get; private set; }

    protected BookingDetail() { }

    public BookingDetail(Guid bookingId, Guid scheduleId, DateTime ticketDate, int quantity, decimal unitPrice)
    {
        BookingId = bookingId;
        ScheduleId = scheduleId;
        TicketDate = ticketDate;
        Quantity = quantity;
        UnitPrice = unitPrice;
        SubTotal = quantity * unitPrice;
    }
}
