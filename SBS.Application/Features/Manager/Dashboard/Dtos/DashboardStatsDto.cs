using System.Collections.Generic;

namespace SBS.Application.Features.Manager.Dashboard.Dtos;

public class DashboardStatsDto
{
    public int TotalPools { get; set; }
    public int ActivePools { get; set; }
    public int ActiveTickets { get; set; }
    
    // Revenue Data
    public decimal PeriodRevenue { get; set; }
    public decimal RevenueGrowthPercentage { get; set; } // +8.5%
    
    // Line Chart: Revenue
    public List<decimal> ChartData { get; set; } = new();
    public List<string> ChartLabels { get; set; } = new();

    // Doughnut Chart: Ticket Distribution
    public List<int> TicketQuantities { get; set; } = new();
    public List<string> TicketLabels { get; set; } = new();
}
