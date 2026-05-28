using System.Collections.Generic;

namespace SBS.Domain.Entities;

public class PoolService
{
    public int PoolServiceId { get; set; }
    public int PoolId { get; set; }
    public string ServiceName { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? ServiceImage { get; set; }
    public int Quantity { get; set; }
    public string ServiceStatus { get; set; } = null!;

    // Navigation properties
    public virtual Pool Pool { get; set; } = null!;
    public virtual ICollection<BookingService> BookingServices { get; set; } = new HashSet<BookingService>();
    public virtual ICollection<ServiceReport> ServiceReports { get; set; } = new HashSet<ServiceReport>();
}
