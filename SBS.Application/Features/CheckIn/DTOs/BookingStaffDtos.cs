namespace SBS.Application.Features.CheckIn.DTOs;

// ── LIST ITEM ─────────────────────────────────────────────────────────────────

public class BookingListItemDto
{
    public int BookingId { get; set; }
    public string? CustomerName { get; set; }
    public DateOnly BookingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int SlotCount { get; set; }
    public string BookingStatus { get; set; } = null!;
    public bool IsCheckedIn { get; set; }
}

// ── NESTED DETAIL TYPES ───────────────────────────────────────────────────────

public class BookingTicketItemDto
{
    public int TicketId { get; set; }
    public string TicketCode { get; set; } = null!;
    public string TicketType { get; set; } = null!;
    public decimal TicketPrice { get; set; }
    public DateTime? IssuedAt { get; set; }
}

public class BookingServiceItemDto
{
    public int BookingServiceId { get; set; }
    public string ServiceName { get; set; } = null!;
    public int? Quantity { get; set; }
    public decimal TotalServicePrice { get; set; }
}

public class BookingCheckinItemDto
{
    public int CheckinId { get; set; }
    public DateTime? CheckinTime { get; set; }
    public string CheckinStatus { get; set; } = null!;
}

// ── DETAIL ────────────────────────────────────────────────────────────────────

public class BookingDetailDto
{
    public int BookingId { get; set; }
    public int? UserId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public int PoolId { get; set; }
    public string PoolName { get; set; } = null!;
    public DateOnly BookingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int SlotCount { get; set; }
    public string BookingStatus { get; set; } = null!;
    public string? DiscountCode { get; set; }
    public IReadOnlyList<BookingTicketItemDto> Tickets { get; set; } = [];
    public IReadOnlyList<BookingServiceItemDto> BookingServices { get; set; } = [];
    public IReadOnlyList<BookingCheckinItemDto> Checkins { get; set; } = [];
}
