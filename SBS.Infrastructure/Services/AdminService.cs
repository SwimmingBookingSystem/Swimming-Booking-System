using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SBS.Application.Common.Dtos.Admin;
using SBS.Application.Common.Dtos.Profile;
using SBS.Application.Common.Interfaces;
using SBS.Infrastructure.Data;
using SBS.Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Infrastructure.Services;

public class AdminService : IAdminService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ReadDbContext _readContext;

    public AdminService(UserManager<AppUser> userManager, ReadDbContext readContext)
    {
        _userManager = userManager;
        _readContext = readContext;
    }

    public async Task<List<UserListDto>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _readContext.Users
            .GroupJoin(
                _readContext.UserRoles,
                u => u.Id,
                ur => ur.UserId,
                (u, userRoles) => new { u, userRoles }
            )
            .SelectMany(
                x => x.userRoles.DefaultIfEmpty(),
                (x, ur) => new { x.u, ur }
            )
            .GroupJoin(
                _readContext.Roles,
                x => x.ur.RoleId,
                r => r.Id,
                (x, roles) => new { x.u, x.ur, roles }
            )
            .SelectMany(
                x => x.roles.DefaultIfEmpty(),
                (x, r) => new { x.u, r }
            )
            .OrderByDescending(x => x.u.CreatedAt)
            .Select(x => new UserListDto
            {
                UserId = x.u.Id,
                UserName = x.u.UserName ?? string.Empty,
                Email = x.u.Email ?? string.Empty,
                FullName = x.u.FullName,
                PhoneNumber = x.u.PhoneNumber,
                Status = x.u.Status,
                Role = x.r.Name ?? "Customer",
                CreatedAt = x.u.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return users;
    }

    public async Task<ResultDto> LockUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return ResultDto.Failure(new[] { "Người dùng không tồn tại." });

        if (user.Status == "Locked")
            return ResultDto.Failure(new[] { "Tài khoản đã bị khóa." });

        user.Status = "Locked";
        user.UpdatedAt = DateTime.UtcNow;
        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
            return ResultDto.Failure(updateResult.Errors.Select(e => e.Description));

        await _userManager.SetLockoutEnabledAsync(user, true);
        await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);

        return ResultDto.Success();
    }

    public async Task<ResultDto> UnlockUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return ResultDto.Failure(new[] { "Người dùng không tồn tại." });

        if (user.Status == "Active")
            return ResultDto.Failure(new[] { "Tài khoản đang hoạt động." });

        user.Status = "Active";
        user.UpdatedAt = DateTime.UtcNow;
        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
            return ResultDto.Failure(updateResult.Errors.Select(e => e.Description));

        await _userManager.SetLockoutEndDateAsync(user, null);

        return ResultDto.Success();
    }

    public async Task<ResultDto> CreateStaffAsync(CreateUserDto dto, CancellationToken cancellationToken = default)
    {
        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = dto.UserName,
            Email = dto.Email,
            FullName = dto.FullName,
            PhoneNumber = dto.PhoneNumber,
            Address = dto.Address,
            Gender = dto.Gender,
            Dob = dto.Dob,
            Status = "Active",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return ResultDto.Failure(result.Errors.Select(e => e.Description));

        await _userManager.AddToRoleAsync(user, "Staff");

        return ResultDto.Success();
    }

    public async Task<ResultDto> CreateManagerAsync(CreateUserDto dto, CancellationToken cancellationToken = default)
    {
        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = dto.UserName,
            Email = dto.Email,
            FullName = dto.FullName,
            PhoneNumber = dto.PhoneNumber,
            Address = dto.Address,
            Gender = dto.Gender,
            Dob = dto.Dob,
            Status = "Active",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return ResultDto.Failure(result.Errors.Select(e => e.Description));

        await _userManager.AddToRoleAsync(user, "Manager");

        return ResultDto.Success();
    }

    public async Task<ResultDto> ChangeUserRoleAsync(Guid userId, string newRole, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return ResultDto.Failure(new[] { "Người dùng không tồn tại." });

        var validRoles = new[] { "Customer", "Staff", "Manager", "Admin" };
        if (!validRoles.Contains(newRole))
            return ResultDto.Failure(new[] { "Vai trò không hợp lệ." });

        var currentRoles = await _userManager.GetRolesAsync(user);
        var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!removeResult.Succeeded)
            return ResultDto.Failure(removeResult.Errors.Select(e => e.Description));

        var addResult = await _userManager.AddToRoleAsync(user, newRole);
        if (!addResult.Succeeded)
            return ResultDto.Failure(addResult.Errors.Select(e => e.Description));

        return ResultDto.Success();
    }

    public async Task<List<RoleDto>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _readContext.Roles
            .Select(r => new RoleDto
            {
                RoleId = r.Id,
                RoleName = r.Name ?? string.Empty
            })
            .ToListAsync(cancellationToken);

        return roles;
    }

    public async Task<DashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var totalRevenue = await _readContext.Payments
            .Where(p => p.Status == "Completed")
            .SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0;

        var totalUsers = await _readContext.Users.CountAsync(cancellationToken);
        var totalBookings = await _readContext.Bookings.CountAsync(cancellationToken);
        var totalPools = await _readContext.Pools.CountAsync(cancellationToken);

        var todayBookings = await _readContext.Bookings
            .CountAsync(b => b.BookingDate == today, cancellationToken);

        var thisMonthRevenue = await _readContext.Payments
            .Where(p => p.Status == "Completed" && p.PaymentDate >= startOfMonth)
            .SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0;

        var newUsersThisMonth = await _readContext.Users
            .CountAsync(u => u.CreatedAt >= startOfMonth, cancellationToken);

        var monthlyRevenues = await _readContext.Payments
            .Where(p => p.Status == "Completed" && p.PaymentDate != null && p.PaymentDate.Value.Year == now.Year)
            .GroupBy(p => new { p.PaymentDate!.Value.Year, p.PaymentDate.Value.Month })
            .Select(g => new MonthlyRevenueDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Revenue = g.Sum(p => p.Amount)
            })
            .OrderBy(m => m.Month)
            .ToListAsync(cancellationToken);

        var usersByRole = await _readContext.Users
            .GroupJoin(
                _readContext.UserRoles,
                u => u.Id,
                ur => ur.UserId,
                (u, userRoles) => new { u, userRoles }
            )
            .SelectMany(
                x => x.userRoles.DefaultIfEmpty(),
                (x, ur) => new { x.u, ur }
            )
            .GroupJoin(
                _readContext.Roles,
                x => x.ur.RoleId,
                r => r.Id,
                (x, roles) => new { x.u, roles }
            )
            .SelectMany(
                x => x.roles.DefaultIfEmpty(),
                (x, r) => new { x.u, r }
            )
            .GroupBy(x => x.r.Name ?? "Customer")
            .Select(g => new UserByRoleDto
            {
                Role = g.Key,
                Count = g.Count()
            })
            .ToListAsync(cancellationToken);

        var bookingsByStatus = await _readContext.Bookings
            .GroupBy(b => b.Status)
            .Select(g => new BookingByStatusDto
            {
                Status = g.Key,
                Count = g.Count()
            })
            .ToListAsync(cancellationToken);

        var recentBookings = await _readContext.Bookings
            .OrderByDescending(b => b.CreatedAt)
            .Take(10)
            .Join(
                _readContext.Users,
                b => b.UserId,
                u => u.Id,
                (b, u) => new RecentBookingDto
                {
                    BookingId = b.BookingId,
                    BookingCode = b.BookingCode,
                    CustomerName = u.FullName,
                    TotalAmount = b.TotalAmount,
                    Status = b.Status,
                    CreatedAt = b.CreatedAt
                }
            )
            .ToListAsync(cancellationToken);

        return new DashboardDto
        {
            Overview = new OverviewDto
            {
                TotalRevenue = totalRevenue,
                TotalUsers = totalUsers,
                TotalBookings = totalBookings,
                TotalPools = totalPools,
                TodayBookings = todayBookings,
                ThisMonthRevenue = thisMonthRevenue,
                NewUsersThisMonth = newUsersThisMonth
            },
            MonthlyRevenues = monthlyRevenues,
            UsersByRole = usersByRole,
            BookingsByStatus = bookingsByStatus,
            RecentBookings = recentBookings
        };
    }
}
