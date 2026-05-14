using SBS.Domain.Enums;
using System;
using System.Collections.Generic;

namespace SBS.Domain.Entities;

public class Booking : BaseEntity
{
    public Guid CustomerId { get; private set; }
    public DateTime BookingDate { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Notes { get; private set; }
    public string CancellationReason { get; private set; }
    public BookingStatus Status { get; private set; }

    // Navigation properties
    public virtual AppUser Customer { get; private set; }
    public virtual ICollection<BookingDetail> BookingDetails { get; private set; }
    public virtual Payment Payment { get; private set; }
    public virtual CheckIn CheckIn { get; private set; }

    protected Booking() 
    {
        BookingDetails = new HashSet<BookingDetail>();
    }

    public Booking(Guid customerId, decimal totalAmount, string notes = null) : this()
    {
        CustomerId = customerId;
        BookingDate = DateTime.UtcNow;
        TotalAmount = totalAmount;
        Notes = notes;
        Status = BookingStatus.Pending;
    }

    public void Cancel(string reason)
    {
        Status = BookingStatus.Cancelled;
        CancellationReason = reason;
        UpdateTimestamp();
    }

    public void UpdateStatus(BookingStatus status)
    {
        Status = status;
        UpdateTimestamp();
    }
}
