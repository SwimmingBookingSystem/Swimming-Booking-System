using System.Collections.Generic;

namespace SBS.Application.Common.Dtos.Customer.CustomerViewPoolList;

public class PoolFilterOptionsDto
{
    public List<string> OpeningTimes { get; set; } = new();
    public List<string> ClosingTimes { get; set; } = new();
}
