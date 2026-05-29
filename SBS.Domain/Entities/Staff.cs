using System.Collections.Generic;

namespace SBS.Domain.Entities;

public class Staff
{
    public int StaffId { get; set; }
    public Guid UserId { get; set; }
    public int BranchId { get; set; }
    public int PoolId { get; set; }
    public int StaffTypeId { get; set; }

    // Navigation properties
    public virtual Branch Branch { get; set; } = null!;
    public virtual Pool Pool { get; set; } = null!;
    public virtual StaffType StaffType { get; set; } = null!;
    public virtual ICollection<ServiceReport> ServiceReports { get; set; } = new HashSet<ServiceReport>();
    public virtual ICollection<DeviceReport> DeviceReports { get; set; } = new HashSet<DeviceReport>();
    public virtual ICollection<SaleTicketDirectly> SaleTicketDirectlys { get; set; } = new HashSet<SaleTicketDirectly>();
}
