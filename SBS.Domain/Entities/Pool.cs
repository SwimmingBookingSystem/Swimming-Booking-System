using SBS.Domain.Enums;
using System.Collections.Generic;

namespace SBS.Domain.Entities;

public class SwimmingPool : BaseEntity
{
    public string Name { get; private set; }
    public string Location { get; private set; }
    public string Description { get; private set; }
    public string ImageUrl { get; private set; }
    public string ContactNumber { get; private set; }
    public string Rules { get; private set; }
    public string Dimension { get; private set; }
    public double Depth { get; private set; }
    public int Capacity { get; private set; }
    public PoolStatus Status { get; private set; }

    // Navigation properties
    public virtual ICollection<PoolSchedule> Schedules { get; private set; }
    public virtual ICollection<PoolImage> Images { get; private set; }
    public virtual ICollection<MaintenanceRequest> MaintenanceRequests { get; private set; }
    public virtual ICollection<Review> Reviews { get; private set; }

    protected SwimmingPool() 
    {
        Schedules = new HashSet<PoolSchedule>();
        Images = new HashSet<PoolImage>();
        MaintenanceRequests = new HashSet<MaintenanceRequest>();
        Reviews = new HashSet<Review>();
    }

    public SwimmingPool(string name, string location, string description, string imageUrl, string dimension, double depth, int capacity) : this()
    {
        Name = name;
        Location = location;
        Description = description;
        ImageUrl = imageUrl;
        Dimension = dimension;
        Depth = depth;
        Capacity = capacity;
        Status = PoolStatus.Active;
    }

    public void UpdateStatus(PoolStatus newStatus)
    {
        Status = newStatus;
        UpdateTimestamp();
    }
}
