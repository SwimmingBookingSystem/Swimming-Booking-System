using SBS.Domain.Enums;
using System;

namespace SBS.Domain.Entities;

public class CheckIn : BaseEntity
{
    public Guid BookingId { get; private set; }
    public DateTime CheckInTime { get; private set; }
    public DateTime? CheckOutTime { get; private set; }
    public Guid ReceptionistId { get; private set; }
    public CheckInStatus Status { get; private set; }

    // Navigation properties
    public virtual Booking Booking { get; private set; }
    public virtual AppUser Receptionist { get; private set; }

    protected CheckIn() { }

    public CheckIn(Guid bookingId, Guid receptionistId)
    {
        BookingId = bookingId;
        ReceptionistId = receptionistId;
        CheckInTime = DateTime.UtcNow;
        Status = CheckInStatus.CheckedIn;
    }

    public void MarkCheckOut()
    {
        CheckOutTime = DateTime.UtcNow;
        Status = CheckInStatus.CheckedOut;
        UpdateTimestamp();
    }
}
