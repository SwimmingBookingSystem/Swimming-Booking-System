namespace SBS.Application.Features.Manager.TicketTypes.Dtos;

public class PoolTicketTypeDto
{
    public int PoolTicketTypeId { get; set; }
    public int PoolId { get; set; }
    public string PoolName { get; set; } = null!;
    public int TicketTypeId { get; set; }
    public string TicketCode { get; set; } = null!;
    public string TicketName { get; set; } = null!;
    public string Category { get; set; } = null!; // "Single" / "Combo"
    public decimal BasePrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal? Price { get; set; }    // Giá riêng tại pool này, null = lấy giá hệ thống
    public string Status { get; set; } = null!;
    public string GlobalTicketStatus { get; set; } = "Active"; // Trạng thái gốc của Loại vé
}
