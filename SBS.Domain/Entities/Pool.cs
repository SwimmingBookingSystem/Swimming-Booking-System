using System;
using System.Collections.Generic;

namespace SBS.Domain.Entities;

public class Pool
{
    public int PoolId { get; set; }
    public string PoolName { get; set; } = null!;
    public string PoolRoad { get; set; } = null!;
    public string PoolAddress { get; set; } = null!;
    public int MaxSlot { get; set; }
    public TimeSpan OpenTime { get; set; }
    public TimeSpan CloseTime { get; set; }
    public bool? PoolStatus { get; set; }
    public string? PoolImage { get; set; }
    public string? PoolDescription { get; set; }
    public int BranchId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual Branch Branch { get; set; } = null!;
    public virtual ICollection<PoolImage> PoolImages { get; set; } = new HashSet<PoolImage>();
    public virtual ICollection<PoolDevice> PoolDevices { get; set; } = new HashSet<PoolDevice>();
    public virtual ICollection<PoolService> PoolServices { get; set; } = new HashSet<PoolService>();
    public virtual ICollection<Booking> Bookings { get; set; } = new HashSet<Booking>();
    public virtual ICollection<Feedback> Feedbacks { get; set; } = new HashSet<Feedback>();
    public virtual ICollection<PoolTicketType> PoolTicketTypes { get; set; } = new HashSet<PoolTicketType>();
    public virtual ICollection<Staff> Staffs { get; set; } = new HashSet<Staff>();
}
