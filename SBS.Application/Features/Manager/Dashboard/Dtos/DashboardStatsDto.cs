using System.Collections.Generic;

namespace SBS.Application.Features.Manager.Dashboard.Dtos;

public class DashboardStatsDto
{
    public int TotalPools { get; set; }
    public int ActivePools { get; set; }
    public int ActiveTickets { get; set; }
    
    // Revenue Data
    public decimal WeeklyRevenue { get; set; }
    public decimal RevenueGrowthPercentage { get; set; } // +8.5%
    
    // Line Chart: 7 Days Revenue
    public List<decimal> RevenueLast7Days { get; set; } = new();
    public List<string> LabelsLast7Days { get; set; } = new();

    // Doughnut Chart: Ticket Distribution
    public List<int> TicketQuantities { get; set; } = new();
    public List<string> TicketLabels { get; set; } = new();
}
