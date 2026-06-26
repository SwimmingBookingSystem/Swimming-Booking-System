namespace SBS.Application.Features.Customer_Bookings.Events;

public class BookingConfirmedEvent
{
    public int BookingId { get; set; }
    public string BookingCode { get; set; } = null!;
    public string UserEmail { get; set; } = null!;
    public string QrCodeData { get; set; } = null!;
}
