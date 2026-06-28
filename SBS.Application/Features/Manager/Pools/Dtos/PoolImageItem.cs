namespace SBS.Application.Features.Manager.Pools.Dtos;

public class PoolImageItem
{
    public string ImageUrl { get; set; } = null!;
    public bool IsCover { get; set; } = false;
    public int SortOrder { get; set; } = 0;
}
