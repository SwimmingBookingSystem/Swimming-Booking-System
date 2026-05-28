using System.Collections.Generic;

namespace SBS.Domain.Entities;

public class StaffType
{
    public int StaffTypeId { get; set; }
    public string TypeName { get; set; } = null!;
    public string? Description { get; set; }

    // Navigation properties
    public virtual ICollection<Staff> Staffs { get; set; } = new HashSet<Staff>();
}
