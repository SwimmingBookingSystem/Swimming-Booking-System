using MediatR;
using Microsoft.EntityFrameworkCore;
using SBS.Application.Common.Interfaces;
using SBS.Application.Features.Manager.Dashboard.Dtos;
using SBS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Features.Manager.Dashboard.Queries.GetDashboardStats;

public record GetDashboardStatsQuery(string TimeRange = "week") : IRequest<DashboardStatsDto>;

public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    private readonly IReadOnlyUnitOfWork _uow;

    public GetDashboardStatsQueryHandler(IReadOnlyUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken ct)
    {
        var dto = new DashboardStatsDto();

        // 1. Basic Counts
        dto.TotalPools = await _uow.Repository<Pool>().Query().CountAsync(ct);
        dto.ActivePools = await _uow.Repository<Pool>().Query().CountAsync(p => p.Status == "Active", ct);
        dto.ActiveTickets = await _uow.Repository<TicketType>().Query().CountAsync(t => t.Status == "Active", ct);

        // 2. Revenue (based on TimeRange)
        var today = DateTime.UtcNow.Date;
        DateTime startDate;
        
        switch (request.TimeRange?.ToLower())
        {
            case "month":
                startDate = new DateTime(today.Year, today.Month, 1);
                break;
            case "year":
                startDate = new DateTime(today.Year, 1, 1);
                break;
            case "week":
            default:
                startDate = today.AddDays(-6);
                break;
        }

        var paymentsPeriod = await _uow.Repository<Payment>().Query()
            .Where(p => p.Status == "Success" && p.PaymentDate >= startDate && p.PaymentDate <= today.AddDays(1))
            .ToListAsync(ct);

        dto.PeriodRevenue = paymentsPeriod.Sum(p => p.Amount);

        // Populate Line Chart Data
        if (request.TimeRange?.ToLower() == "year")
        {
            for (int i = 1; i <= 12; i++)
            {
                var monthTotal = paymentsPeriod
                    .Where(p => p.PaymentDate.HasValue && p.PaymentDate.Value.Month == i)
                    .Sum(p => p.Amount);
                dto.ChartData.Add(monthTotal / 1000000m);
                dto.ChartLabels.Add($"Tháng {i}");
            }
        }
        else if (request.TimeRange?.ToLower() == "month")
        {
            var daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
            for (int i = 1; i <= daysInMonth; i++)
            {
                var dailyTotal = paymentsPeriod
                    .Where(p => p.PaymentDate.HasValue && p.PaymentDate.Value.Day == i)
                    .Sum(p => p.Amount);
                dto.ChartData.Add(dailyTotal / 1000000m);
                dto.ChartLabels.Add($"{i:00}/{today.Month:00}");
            }
        }
        else
        {
            for (int i = 6; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var dailyTotal = paymentsPeriod
                    .Where(p => p.PaymentDate.HasValue && p.PaymentDate.Value.Date == date)
                    .Sum(p => p.Amount);
                dto.ChartData.Add(dailyTotal / 1000000m);
                dto.ChartLabels.Add(date.ToString("dd/MM"));
            }
        }

        // Mock growth percentage
        dto.RevenueGrowthPercentage = dto.PeriodRevenue > 0 ? 8.5m : 0; 

        // 3. Ticket Distribution (Doughnut Chart)
        var bookingDetails = await _uow.Repository<BookingDetail>().Query()
            .Include(bd => bd.Booking)
            .Include(bd => bd.PoolTicketType)
                .ThenInclude(ptt => ptt.TicketType)
            .Where(bd => bd.Booking.Status == "Paid")
            .ToListAsync(ct);

        var ticketGroups = bookingDetails
            .GroupBy(bd => bd.PoolTicketType.TicketType.TicketName)
            .Select(g => new { 
                Name = g.Key, 
                Count = g.Sum(bd => bd.Quantity) 
            })
            .OrderByDescending(x => x.Count)
            .Take(5) // Top 5 types
            .ToList();

        foreach (var group in ticketGroups)
        {
            var labelName = group.Name;
            if (labelName.Contains("Vé cá nhân", StringComparison.OrdinalIgnoreCase))
            {
                labelName = labelName.Replace("Vé cá nhân", "Vé đơn", StringComparison.OrdinalIgnoreCase);
            }

            dto.TicketLabels.Add(labelName);
            dto.TicketQuantities.Add(group.Count);
        }

        return dto;
    }
}
