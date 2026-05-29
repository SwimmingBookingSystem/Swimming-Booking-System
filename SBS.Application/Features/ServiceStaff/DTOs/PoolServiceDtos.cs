namespace SBS.Application.Features.ServiceStaff.DTOs;

public class PoolServiceDto
{
    public int PoolServiceId { get; set; }
    public int PoolId { get; set; }
    public string? PoolName { get; set; }
    public string ServiceName { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? ServiceImage { get; set; }
    public int Quantity { get; set; }
    public string ServiceStatus { get; set; } = null!;
}
