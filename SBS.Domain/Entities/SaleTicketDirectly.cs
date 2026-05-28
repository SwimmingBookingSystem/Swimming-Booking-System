using System;

namespace SBS.Domain.Entities;

public class SaleTicketDirectly
{
    public int SaleId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerEmail { get; set; }
    public int? UserId { get; set; }
    public int StaffId { get; set; }
    public int BookingId { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public string PaymentStatus { get; set; } = null!;
    public DateTime SaleDate { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Staff Staff { get; set; } = null!;
    public virtual Booking Booking { get; set; } = null!;
}
