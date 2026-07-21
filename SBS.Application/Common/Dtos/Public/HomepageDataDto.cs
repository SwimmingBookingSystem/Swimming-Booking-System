using System;
using System.Collections.Generic;

namespace SBS.Application.Common.Dtos.Public;

public class HomepageDataDto
{
    public HomepageStatsDto Stats { get; set; } = null!;
    public List<HomepagePoolDto> Pools { get; set; } = new();
}

public class HomepageStatsDto
{
    public int TotalPools { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalBookings { get; set; }
    public double AverageRating { get; set; }
}

public class HomepagePoolDto
{
    public int PoolId { get; set; }
    public string PoolName { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }
    public string OpeningTime { get; set; } = null!;
    public string ClosingTime { get; set; } = null!;
    public decimal? MinPrice { get; set; }
}
