using System;
using System.Collections.Generic;

namespace SBS.Domain.Entities;

public class TicketType
{
    public int TicketTypeId { get; set; }
    public string TypeCode { get; set; } = null!;
    public string TypeName { get; set; } = null!;
    public string? Description { get; set; }
    public decimal BasePrice { get; set; }
    public bool? IsCombo { get; set; }
    public decimal? DiscountPercent { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<ComboDetail> ComboDetails { get; set; } = new HashSet<ComboDetail>();
    public virtual ICollection<PoolTicketType> PoolTicketTypes { get; set; } = new HashSet<PoolTicketType>();
    public virtual ICollection<Ticket> Tickets { get; set; } = new HashSet<Ticket>();
}
