namespace SBS.Application.Features.CheckIn.DTOs;

// ── REQUEST ──────────────────────────────────────────────────────────────────

public class VerifyTicketRequestDto
{
    /// <summary>Ticket code from QR scan or manual input.</summary>
    public string TicketCode { get; set; } = null!;
}

// ── RESPONSE ─────────────────────────────────────────────────────────────────

public class VerifyTicketResponseDto
{
    public bool IsValid { get; set; }

    // Populated when IsValid = true
    public int? TicketId { get; set; }
    public string? TicketCode { get; set; }
    public string? TicketType { get; set; }
    public DateOnly? BookingDate { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public string? PoolName { get; set; }
    public string? CustomerName { get; set; }
    public int? SlotCount { get; set; }
    public string? BookingStatus { get; set; }

    // Populated when IsValid = false
    public string? Reason { get; set; }
}
