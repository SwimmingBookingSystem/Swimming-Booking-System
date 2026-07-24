using System;

namespace SBS.Application.Features.Customer_Bookings.Dtos;

public class CustomerBookingHistoryDto
{
    public int BookingId { get; set; }
    public string BookingCode { get; set; } = null!;
    public string PoolName { get; set; } = null!;
    public DateOnly SlotDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Status { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public string? QrCodeData { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<CustomerBookingHistoryTicketDto> Tickets { get; set; } = new();
}

public class CustomerBookingHistoryTicketDto
{
    public string TicketName { get; set; } = null!;
    public string Category { get; set; } = null!;
    public int Quantity { get; set; }
}
