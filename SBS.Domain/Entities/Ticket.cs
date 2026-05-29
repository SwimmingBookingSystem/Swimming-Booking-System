using System;

namespace SBS.Domain.Entities;

public class Ticket
{
    public int TicketId { get; set; }
    public int BookingId { get; set; }
    public int TicketTypeId { get; set; }
    public decimal TicketPrice { get; set; }
    public string TicketCode { get; set; } = null!;
    public Guid? IssuedBy { get; set; }
    public DateTime? IssuedAt { get; set; }

    // Navigation properties
    public virtual Booking Booking { get; set; } = null!;
    public virtual TicketType TicketType { get; set; } = null!;
}
