using System;
using System.Collections.Generic;

namespace SBS.Domain.Entities;

public class TicketType
{
    public int TicketTypeId { get; set; }
    public string TicketCode { get; set; } = null!;
    public string TicketName { get; set; } = null!;
    public string Category { get; set; } = null!; // "Single" or "Combo"
    public decimal BasePrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<PoolTicketType> PoolTicketTypes { get; set; } = new List<PoolTicketType>();

    // Combo relationship: this ticket is a combo containing these single tickets
    public virtual ICollection<ComboDetail> ComboItems { get; set; } = new List<ComboDetail>();

    // Reverse: this single ticket is included in these combos
    public virtual ICollection<ComboDetail> IncludedInCombos { get; set; } = new List<ComboDetail>();

    public virtual ICollection<TicketPriceHistory> PriceHistories { get; set; } = new List<TicketPriceHistory>();
}
