using System;

namespace SBS.Domain.Entities;

public class PoolImage : BaseEntity
{
    public Guid PoolId { get; private set; }
    public string ImageUrl { get; private set; }
    public string Caption { get; private set; }
    public bool IsPrimary { get; private set; }

    // Navigation property
    public virtual SwimmingPool Pool { get; private set; }

    protected PoolImage() { }

    public PoolImage(Guid poolId, string imageUrl, string caption = null, bool isPrimary = false)
    {
        PoolId = poolId;
        ImageUrl = imageUrl;
        Caption = caption;
        IsPrimary = isPrimary;
    }
}
