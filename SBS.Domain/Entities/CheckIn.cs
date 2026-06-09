using System;

namespace SBS.Domain.Entities;

public class CheckIn
{
    public int CheckInId { get; set; }
    public int BookingId { get; set; }
    public Guid CheckedByUserId { get; set; } // FK → AppUser (Staff)
    public string CheckInMethod { get; set; } = null!; // QR, Manual
    public DateTime CheckInTime { get; set; } = DateTime.UtcNow;

    // Navigation properties (domain entities only, no AppUser)
    public virtual Booking Booking { get; set; } = null!;
}
