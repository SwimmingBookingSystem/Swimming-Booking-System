namespace SBS.Application.Features.CheckIn.DTOs;

// ── REQUEST ──────────────────────────────────────────────────────────────────

public class CheckInRequestDto
{
    /// <summary>Ticket code from QR scan.</summary>
    public string TicketCode { get; set; } = null!;

    /// <summary>Booking ID associated with the ticket.</summary>
    public int BookingId { get; set; }

    /// <summary>
    /// StaffId of the staff member performing check-in.
    /// TODO: Replace with JWT claim when auth is configured by auth team.
    /// </summary>
    public int StaffId { get; set; }
}

// ── RESPONSE ─────────────────────────────────────────────────────────────────

public class CheckInResponseDto
{
    public int CheckinId { get; set; }
    public int BookingId { get; set; }
    public DateTime CheckinTime { get; set; }
    public string CheckinStatus { get; set; } = null!;
    public string Message { get; set; } = null!;
}
