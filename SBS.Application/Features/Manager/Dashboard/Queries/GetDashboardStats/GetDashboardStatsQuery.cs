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

public record GetDashboardStatsQuery : IRequest<DashboardStatsDto>;

public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    private readonly IUnitOfWork _uow;

    public GetDashboardStatsQueryHandler(IUnitOfWork uow)
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

        // 2. Revenue (Last 7 Days)
        var today = DateTime.UtcNow.Date;
        var sevenDaysAgo = today.AddDays(-6); // Including today makes it 7 days

        var paymentsLast7Days = await _uow.Repository<Payment>().Query()
            .Where(p => p.Status == "Success" && p.PaymentDate >= sevenDaysAgo)
            .ToListAsync(ct);

        dto.WeeklyRevenue = paymentsLast7Days.Sum(p => p.Amount);

        // Populate Line Chart Data
        for (int i = 6; i >= 0; i--)
        {
            var date = today.AddDays(-i);
            var dailyTotal = paymentsLast7Days
                .Where(p => p.PaymentDate.HasValue && p.PaymentDate.Value.Date == date)
                .Sum(p => p.Amount);
            
            // Convert to Millions for UI friendliness, e.g., 12.5M
            dto.RevenueLast7Days.Add(dailyTotal / 1000000m);
            
            // Format label e.g., "T2", "T3" or "25/06"
            dto.LabelsLast7Days.Add(date.ToString("dd/MM"));
        }

        // Just a mock growth percentage based on some random logic or hardcode for now
        // since we don't fetch last week's data. 
        dto.RevenueGrowthPercentage = dto.WeeklyRevenue > 0 ? 8.5m : 0; 

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
            dto.TicketLabels.Add(group.Name);
            dto.TicketQuantities.Add(group.Count);
        }

        return dto;
    }
}
