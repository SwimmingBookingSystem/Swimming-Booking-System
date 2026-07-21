using System;

namespace SBS.Application.Common.Dtos.Staff;

public record BookingListItemDto
{
    public int BookingId { get; init; }
    public string BookingCode { get; init; } = null!;
    public string CustomerName { get; init; } = null!;
    public string CustomerEmail { get; init; } = null!;
    public string? CustomerPhone { get; init; }
    public string PoolName { get; init; } = null!;
    public string SlotTime { get; init; } = null!;   // "08:00 - 09:00"
    public DateOnly BookingDate { get; init; }
    public string Status { get; init; } = null!;
    public string StatusDisplay { get; init; } = null!;  // Tiếng Việt: "Đã thanh toán", "Đã check-in", ...
    public decimal TotalAmount { get; init; }
    public string BookingType { get; init; } = null!; // Online / WalkIn
    public DateTime CreatedAt { get; init; }
}
