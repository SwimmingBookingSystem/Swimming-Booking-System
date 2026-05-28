using System;
using System.Collections.Generic;

namespace SBS.Domain.Entities;

public class Booking
{
    public int BookingId { get; set; }
    public int? UserId { get; set; }
    public int PoolId { get; set; }
    public int? DiscountId { get; set; }
    public DateOnly BookingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int SlotCount { get; set; }
    public string BookingStatus { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual Pool Pool { get; set; } = null!;
    public virtual Discount? Discount { get; set; }
    public virtual ICollection<BookingService> BookingServices { get; set; } = new HashSet<BookingService>();
    public virtual ICollection<Feedback> Feedbacks { get; set; } = new HashSet<Feedback>();
    public virtual ICollection<Payment> Payments { get; set; } = new HashSet<Payment>();
    public virtual ICollection<Ticket> Tickets { get; set; } = new HashSet<Ticket>();
    public virtual ICollection<CustomerCheckin> CustomerCheckins { get; set; } = new HashSet<CustomerCheckin>();
    public virtual ICollection<SaleTicketDirectly> SaleTicketDirectlys { get; set; } = new HashSet<SaleTicketDirectly>();
}
