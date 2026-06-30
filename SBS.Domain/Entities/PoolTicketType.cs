using System.Collections.Generic;

namespace SBS.Domain.Entities;

public class PoolTicketType
{
    public int PoolTicketTypeId { get; set; }
    public int PoolId { get; set; }
    public int TicketTypeId { get; set; }
    public decimal? Price { get; set; }
    public string Status { get; set; } = "Active";

    // Navigation properties
    public virtual Pool Pool { get; set; } = null!;
    public virtual TicketType TicketType { get; set; } = null!;
    public virtual ICollection<BookingDetail> BookingDetails { get; set; } = new List<BookingDetail>();
    public virtual ICollection<PoolTicketPriceHistory> PriceHistories { get; set; } = new List<PoolTicketPriceHistory>();
}
