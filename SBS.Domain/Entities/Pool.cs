using System;
using System.Collections.Generic;

namespace SBS.Domain.Entities;

public class Pool
{
    public int PoolId { get; set; }
    public string PoolName { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public TimeSpan OpeningTime { get; set; }
    public TimeSpan ClosingTime { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<PoolSlot> PoolSlots { get; set; } = new List<PoolSlot>();
    public virtual ICollection<PoolTicketType> PoolTicketTypes { get; set; } = new List<PoolTicketType>();
    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
}
