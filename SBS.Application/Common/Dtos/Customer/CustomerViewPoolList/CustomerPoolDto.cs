namespace SBS.Application.Common.Dtos.Customer.CustomerViewPoolList;

public class CustomerPoolDto
{
    public int PoolId { get; set; }
    public string PoolName { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }
    public string OpeningTime { get; set; } = null!;
    public string ClosingTime { get; set; } = null!;
}
