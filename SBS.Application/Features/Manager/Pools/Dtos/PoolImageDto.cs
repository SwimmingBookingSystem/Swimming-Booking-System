using System;
using System.Collections.Generic;

namespace SBS.Application.Features.Manager.Pools.Dtos;

public class PoolImageDto
{
    public int PoolImageId { get; set; }
    public string ImageUrl { get; set; } = null!;
    public bool IsCover { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}
