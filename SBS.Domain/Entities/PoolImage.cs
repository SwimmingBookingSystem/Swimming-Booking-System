namespace SBS.Domain.Entities;

public class PoolImage
{
    public int ImageId { get; set; }
    public int PoolId { get; set; }
    public string PoolImageLink { get; set; } = null!; // maps to pool_image column

    // Navigation properties
    public virtual Pool Pool { get; set; } = null!;
}
