namespace SBS.Application.Features.Customer_Bookings.Dtos;

public class CustomerPoolTicketDto
{
    public int PoolTicketTypeId { get; set; }
    public string TicketName { get; set; } = null!;
    public string Category { get; set; } = null!;
    public decimal? Price { get; set; }
    public string? Description { get; set; }
}
