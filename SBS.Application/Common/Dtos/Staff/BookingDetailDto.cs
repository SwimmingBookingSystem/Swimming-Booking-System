using System;
using System.Collections.Generic;

namespace SBS.Application.Common.Dtos.Staff;

public record BookingDetailDto
{
    // --- Booking info ---
    public int BookingId { get; init; }
    public string BookingCode { get; init; } = null!;
    public string Status { get; init; } = null!;
    public string StatusDisplay { get; init; } = null!;  // Tiếng Việt
    public DateOnly BookingDate { get; init; }
    public decimal TotalAmount { get; init; }
    public string BookingType { get; init; } = null!;
    public DateTime? PaymentDeadline { get; init; }
    public DateTime CreatedAt { get; init; }

    // --- Customer info (từ Identity) ---
    public Guid CustomerId { get; init; }
    public string CustomerName { get; init; } = null!;
    public string CustomerEmail { get; init; } = null!;
    public string? CustomerPhone { get; init; }

    // --- Slot info ---
    public string PoolName { get; init; } = null!;
    public string SlotTime { get; init; } = null!;  // "08:00 - 09:00"

    // --- Ticket details ---
    public List<BookingTicketItemDto> Tickets { get; init; } = new();

    // --- Payment ---
    public PaymentInfoDto? Payment { get; init; }

    // --- CheckIn ---
    public CheckInInfoDto? CheckIn { get; init; }
}

public record BookingTicketItemDto
{
    public string TicketName { get; init; } = null!;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal SubTotal { get; init; }
}

public record PaymentInfoDto
{
    public string PaymentMethod { get; init; } = null!;
    public string Status { get; init; } = null!;
    public string StatusDisplay { get; init; } = null!;  // Tiếng Việt: "Thành công"
    public decimal Amount { get; init; }
    public DateTime? PaymentDate { get; init; }
}

public record CheckInInfoDto
{
    public string CheckInMethod { get; init; } = null!;
    public DateTime CheckInTime { get; init; }
}
