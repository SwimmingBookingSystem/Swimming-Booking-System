using System;
using System.Collections.Generic;

namespace SBS.Domain.Entities;

public class Booking
{
    public int BookingId { get; set; }
    public string BookingCode { get; set; } = null!;
    public Guid UserId { get; set; }
    public int PoolSlotId { get; set; }
    public DateOnly BookingDate { get; set; }
    public string Status { get; set; } = "PendingPayment";
    public decimal TotalAmount { get; set; }
    public string? QrCodeData { get; set; }
    public string BookingType { get; set; } = "Online";
    public DateTime? PaymentDeadline { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties (domain entities only, no AppUser)
    public virtual PoolSlot PoolSlot { get; set; } = null!;
    public virtual ICollection<BookingDetail> BookingDetails { get; set; } = new List<BookingDetail>();
    public virtual Payment? Payment { get; set; }
    public virtual CheckIn? CheckIn { get; set; }
    public virtual Feedback? Feedback { get; set; }
}
