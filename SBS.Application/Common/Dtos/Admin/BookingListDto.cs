using System;

namespace SBS.Application.Common.Dtos.Admin;

public class BookingListDto
{
    public int BookingId { get; set; }
    public string BookingCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public string PoolName { get; set; } = string.Empty;
    public string? SlotName { get; set; }
    public string SlotTime { get; set; } = string.Empty;
    public DateOnly BookingDate { get; set; }
    public string BookingType { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? PaymentStatus { get; set; }
    public string? PaymentMethod { get; set; }
}
