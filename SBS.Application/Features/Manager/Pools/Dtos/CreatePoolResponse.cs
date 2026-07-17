namespace SBS.Application.Features.Manager.Pools.Dtos;

public class CreatePoolResponse
{
    public int PoolId { get; set; }
    public string PoolName { get; set; } = null!;
    public string Status { get; set; } = null!;
}
