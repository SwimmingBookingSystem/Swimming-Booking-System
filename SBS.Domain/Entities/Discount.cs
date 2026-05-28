using System;
using System.Collections.Generic;

namespace SBS.Domain.Entities;

public class Discount
{
    public int DiscountId { get; set; }
    public string DiscountCode { get; set; } = null!;
    public string? Description { get; set; }
    public decimal DiscountPercent { get; set; }
    public int? Quantity { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public bool? Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public int CreatedBy { get; set; }

    // Navigation properties
    public virtual ICollection<Booking> Bookings { get; set; } = new HashSet<Booking>();
    public virtual ICollection<DiscountAuditLog> DiscountAuditLogs { get; set; } = new HashSet<DiscountAuditLog>();
}
