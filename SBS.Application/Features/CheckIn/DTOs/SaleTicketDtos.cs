namespace SBS.Application.Features.CheckIn.DTOs;

// ── REQUEST ──────────────────────────────────────────────────────────────────

public class SaleTicketRequestDto
{
    public int PoolId { get; set; }
    public int TicketTypeId { get; set; }
    public int SlotCount { get; set; }
    public DateOnly BookingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }

    /// <summary>Payment method: cash | transfer | pos</summary>
    public string PaymentMethod { get; set; } = null!;

    // Walk-in customer info (optional when customer has account via UserId)
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerEmail { get; set; }

    /// <summary>Registered customer UserId (null for walk-in guests).</summary>
    public int? UserId { get; set; }

    /// <summary>
    /// StaffId of the staff member performing the sale.
    /// TODO: Replace with JWT claim when auth is configured by auth team.
    /// </summary>
    public int StaffId { get; set; }

    public int? DiscountId { get; set; }
    public string? Notes { get; set; }
}

// ── RESPONSE ─────────────────────────────────────────────────────────────────

public class SaleTicketResponseDto
{
    public int SaleId { get; set; }
    public int BookingId { get; set; }
    public string TicketCode { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public string PaymentStatus { get; set; } = null!;
    public DateTime SaleDate { get; set; }
    public string? CustomerName { get; set; }
    public int StaffId { get; set; }
    public string Message { get; set; } = null!;
}

// ── LIST ITEM ─────────────────────────────────────────────────────────────────

public class SaleTicketListItemDto
{
    public int SaleId { get; set; }
    public string TicketCode { get; set; } = null!;
    public string? CustomerName { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public string PaymentStatus { get; set; } = null!;
    public DateTime SaleDate { get; set; }
}
