namespace SBS.Application.Features.Manager.TicketTypes.Dtos;

public class PoolTicketTypeDto
{
    public int PoolTicketTypeId { get; set; }
    public int PoolId { get; set; }
    public string PoolName { get; set; } = null!;
    public int TicketTypeId { get; set; }
    public string TicketCode { get; set; } = null!;
    public string TicketName { get; set; } = null!;
    public string Category { get; set; } = null!;
    public decimal BasePrice { get; set; }
    public decimal Price { get; set; }    // Giá riêng tại pool này
    public string Status { get; set; } = null!;
}
