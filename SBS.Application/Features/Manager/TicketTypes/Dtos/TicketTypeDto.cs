namespace SBS.Application.Features.Manager.TicketTypes.Dtos;

public class TicketTypeDto
{
    public int TicketTypeId { get; set; }
    public string TicketCode { get; set; } = null!;
    public string TicketName { get; set; } = null!;
    public string Category { get; set; } = null!;
    public decimal BasePrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public List<ComboDetailDto>? ComboDetails { get; set; }
}

public class ComboDetailDto
{
    public int ComboDetailId { get; set; }
    public int SingleTicketTypeId { get; set; }
    public string SingleTicketCode { get; set; } = null!;
    public string SingleTicketName { get; set; } = null!;
    public int Quantity { get; set; }
}

public class CreateTicketTypeResponse
{
    public int TicketTypeId { get; set; }
    public string TicketCode { get; set; } = null!;
    public string TicketName { get; set; } = null!;
    public string Category { get; set; } = null!;
    public decimal BasePrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public string Status { get; set; } = null!;
}
