namespace SBS.Domain.Entities;

public class ComboDetail
{
    public int ComboDetailId { get; set; }
    public int ComboTicketTypeId { get; set; }
    public int SingleTicketTypeId { get; set; }
    public int Quantity { get; set; }

    // Navigation properties
    public virtual TicketType ComboTicketType { get; set; } = null!;
    public virtual TicketType SingleTicketType { get; set; } = null!;
}
