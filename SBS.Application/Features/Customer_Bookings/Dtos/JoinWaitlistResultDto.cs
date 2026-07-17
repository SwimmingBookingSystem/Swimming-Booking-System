namespace SBS.Application.Features.Customer_Bookings.Dtos;

public record JoinWaitlistResultDto
{
    public bool Succeeded { get; init; }
    public string? Message { get; init; }
}
