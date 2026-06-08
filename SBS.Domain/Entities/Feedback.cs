using System;

namespace SBS.Domain.Entities;

public class Feedback
{
    public int FeedbackId { get; set; }
    public Guid UserId { get; set; } // FK → AppUser
    public int PoolId { get; set; }
    public int? BookingId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties (domain entities only, no AppUser)
    public virtual Pool Pool { get; set; } = null!;
    public virtual Booking? Booking { get; set; }
}
