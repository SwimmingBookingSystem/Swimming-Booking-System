namespace SBS.Domain.Entities;

public class ComboDetail
{
    public int ComboTypeId { get; set; }
    public int IncludedTypeId { get; set; }
    public int Quantity { get; set; }

    // Navigation properties
    public virtual TicketType ComboType { get; set; } = null!;
    public virtual TicketType IncludedType { get; set; } = null!;
}
