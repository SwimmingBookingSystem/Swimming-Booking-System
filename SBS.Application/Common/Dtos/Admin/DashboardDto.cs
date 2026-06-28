using System;
using System.Collections.Generic;

namespace SBS.Application.Common.Dtos.Admin;

public class DashboardDto
{
    public OverviewDto Overview { get; set; } = null!;
    public List<MonthlyRevenueDto> MonthlyRevenues { get; set; } = new();
    public List<UserByRoleDto> UsersByRole { get; set; } = new();
    public List<BookingByStatusDto> BookingsByStatus { get; set; } = new();
    public List<RecentBookingDto> RecentBookings { get; set; } = new();
}

public class OverviewDto
{
    public decimal TotalRevenue { get; set; }
    public int TotalUsers { get; set; }
    public int TotalBookings { get; set; }
    public int TotalPools { get; set; }
    public int TodayBookings { get; set; }
    public decimal ThisMonthRevenue { get; set; }
    public int NewUsersThisMonth { get; set; }
}

public class MonthlyRevenueDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Revenue { get; set; }
}

public class UserByRoleDto
{
    public string Role { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class BookingByStatusDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class RecentBookingDto
{
    public int BookingId { get; set; }
    public string BookingCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
