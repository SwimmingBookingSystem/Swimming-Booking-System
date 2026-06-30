using System;

namespace SBS.Domain.Entities;

public class PoolTicketPriceHistory
{
    public int Id { get; set; }
    public int PoolTicketTypeId { get; set; }
    public decimal? OldCustomPrice { get; set; }
    public decimal? NewCustomPrice { get; set; }
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
    public string? ModifiedByUserName { get; set; }

    public virtual PoolTicketType PoolTicketType { get; set; } = null!;
}
