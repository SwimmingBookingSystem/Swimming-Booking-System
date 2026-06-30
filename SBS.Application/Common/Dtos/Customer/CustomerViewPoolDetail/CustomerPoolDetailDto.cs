using System.Collections.Generic;

namespace SBS.Application.Common.Dtos.Customer.CustomerViewPoolDetail;

public class CustomerPoolDetailDto
{
    public int PoolId { get; set; }
    public string PoolName { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string? Description { get; set; }
    public string OpeningTime { get; set; } = null!; // Format: "hh:mm"
    public string ClosingTime { get; set; } = null!; // Format: "hh:mm"
    public List<string> Images { get; set; } = new(); // Sorted List of image URLs
    public int StandardCapacity { get; set; }
}
