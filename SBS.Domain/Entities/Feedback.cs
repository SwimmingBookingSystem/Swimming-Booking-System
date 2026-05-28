using System;

namespace SBS.Domain.Entities;

public class Feedback
{
    public int FeedbackId { get; set; }
    public int UserId { get; set; }
    public int PoolId { get; set; }
    public int BookingId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public bool? Replied { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Booking Booking { get; set; } = null!;
    public virtual Pool Pool { get; set; } = null!;
}
