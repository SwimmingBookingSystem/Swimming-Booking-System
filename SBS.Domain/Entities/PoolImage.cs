using System;

namespace SBS.Domain.Entities;

public class PoolImage
{
    public int PoolImageId { get; set; }
    public int PoolId { get; set; }
    public string ImageUrl { get; set; } = null!;
    public bool IsCover { get; set; } = false;      // Ảnh bìa chính
    public int SortOrder { get; set; } = 0;         // Thứ tự hiển thị
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public virtual Pool Pool { get; set; } = null!;
}
