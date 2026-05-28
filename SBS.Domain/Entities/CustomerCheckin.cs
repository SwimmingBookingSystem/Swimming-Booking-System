using System;

namespace SBS.Domain.Entities;

public class CustomerCheckin
{
    public int CheckinId { get; set; }
    public int UserId { get; set; }
    public int BookingId { get; set; }
    public byte? CheckinStatus { get; set; }
    public DateTime? CheckinTime { get; set; }

    // Navigation properties
    public virtual Booking Booking { get; set; } = null!;
}
