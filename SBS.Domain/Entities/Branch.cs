using System.Collections.Generic;

namespace SBS.Domain.Entities;

public class Branch
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = null!;
    public Guid? ManagerId { get; set; }

    // Navigation properties
    public virtual ICollection<Pool> Pools { get; set; } = new HashSet<Pool>();
    public virtual ICollection<Staff> Staffs { get; set; } = new HashSet<Staff>();
}
