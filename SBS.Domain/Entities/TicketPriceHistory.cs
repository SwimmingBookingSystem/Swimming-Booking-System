using System;

namespace SBS.Domain.Entities;

public class TicketPriceHistory
{
    public int Id { get; set; }
    public int TicketTypeId { get; set; }
    public decimal OldBasePrice { get; set; }
    public decimal NewBasePrice { get; set; }
    public decimal OldDiscountPercent { get; set; }
    public decimal NewDiscountPercent { get; set; }
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
    public string? ModifiedByUserName { get; set; }

    public virtual TicketType TicketType { get; set; } = null!;
}
