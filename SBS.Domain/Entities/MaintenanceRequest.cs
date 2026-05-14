using SBS.Domain.Enums;
using System;

namespace SBS.Domain.Entities;

public class MaintenanceRequest : BaseEntity
{
    public Guid PoolId { get; private set; }
    public string Description { get; private set; }
    public Guid RequestedById { get; private set; }
    public DateTime RequestDate { get; private set; }
    public DateTime? ResolvedDate { get; private set; }
    public MaintenanceStatus Status { get; private set; }

    // Navigation properties
    public virtual SwimmingPool Pool { get; private set; }
    public virtual AppUser RequestedBy { get; private set; }

    protected MaintenanceRequest() { }

    public MaintenanceRequest(Guid poolId, string description, Guid requestedById)
    {
        PoolId = poolId;
        Description = description;
        RequestedById = requestedById;
        RequestDate = DateTime.UtcNow;
        Status = MaintenanceStatus.Pending;
    }

    public void UpdateStatus(MaintenanceStatus status)
    {
        Status = status;
        if (status == MaintenanceStatus.Resolved)
        {
            ResolvedDate = DateTime.UtcNow;
        }
        UpdateTimestamp();
    }
}
