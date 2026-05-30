using System.Collections.Generic;

namespace SBS.Domain.Entities;

public class PoolDevice
{
    public int DeviceId { get; set; }
    public int PoolId { get; set; }
    public string? DeviceImage { get; set; }
    public string DeviceName { get; set; } = null!;
    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }  // Tuấn Anh thêm 
    public string DeviceStatus { get; set; } = null!;
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Pool Pool { get; set; } = null!;
    public virtual ICollection<DeviceReport> DeviceReports { get; set; } = new HashSet<DeviceReport>();
}
