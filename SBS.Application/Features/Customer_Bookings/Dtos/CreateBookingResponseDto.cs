namespace SBS.Application.Features.Customer_Bookings.Dtos;

public class CreateBookingResponseDto
{
    public int BookingId { get; set; }
    public string BookingCode { get; set; } = null!;
    public string PaymentLink { get; set; } = null!;
}
