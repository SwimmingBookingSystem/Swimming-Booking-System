namespace SBS.Application.Features.CheckIn.DTOs;

public class TicketTypeDto
{
    public int TicketTypeId { get; set; }
    public string TypeCode { get; set; } = null!;
    public string TypeName { get; set; } = null!;
    public string? Description { get; set; }
    public decimal BasePrice { get; set; }
    public bool IsCombo { get; set; }
    public decimal? DiscountPercent { get; set; }
}
