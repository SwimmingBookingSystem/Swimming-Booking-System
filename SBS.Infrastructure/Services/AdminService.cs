using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SBS.Application.Common.Dtos;
using SBS.Application.Common.Dtos.Admin;
using SBS.Application.Common.Dtos.Profile;
using SBS.Application.Common.Interfaces;
using SBS.Domain.Entities;
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
    private readonly ApplicationDbContext _writeContext;
    private readonly ReadDbContext _readContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailService _emailService;

    public AdminService(
        UserManager<AppUser> userManager,
        ApplicationDbContext writeContext,
        ReadDbContext readContext,
        ICurrentUserService currentUserService,
        IEmailService emailService)
    {
        _userManager = userManager;
        _writeContext = writeContext;
        _readContext = readContext;
        _currentUserService = currentUserService;
        _emailService = emailService;
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
                x => x.ur!.RoleId,
                r => r.Id,
                (x, roles) => new { x.u, x.ur, roles }
            )
            .SelectMany(
                x => x.roles.DefaultIfEmpty(),
                (x, r) => new { x.u, r }
            )
            .GroupJoin(
                _readContext.PoolStaffAssignments,
                x => x.u.Id,
                a => a.StaffId,
                (x, assignments) => new { x.u, x.r, assignments }
            )
            .SelectMany(
                x => x.assignments.DefaultIfEmpty(),
                (x, a) => new { x.u, x.r, a }
            )
            .OrderByDescending(x => x.u.CreatedAt)
            .Select(x => new UserListDto
            {
                UserId = x.u.Id,
                UserName = x.u.UserName ?? string.Empty,
                Email = x.u.Email ?? string.Empty,
                FullName = x.u.FullName,
                PhoneNumber = x.u.PhoneNumber,
                Gender = x.u.Gender,
                Dob = x.u.Dob.HasValue ? x.u.Dob.Value.ToString("yyyy-MM-dd") : null,
                Address = x.u.Address,
                PoolId = x.a != null ? x.a.PoolId : (int?)null,
                Status = x.u.Status,
                Role = x.r!.Name ?? "Customer",
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

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Contains("Manager"))
        {
            var activeManagerExists = await _readContext.Users
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
                    x => x.ur!.RoleId,
                    r => r.Id,
                    (x, rolesJoin) => new { x.u, rolesJoin }
                )
                .SelectMany(
                    x => x.rolesJoin.DefaultIfEmpty(),
                    (x, r) => new { x.u, RoleName = r!.Name }
                )
                .AnyAsync(x => x.RoleName == "Manager" && x.u.Status == "Active" && x.u.Id != userId, cancellationToken);

            if (activeManagerExists)
                return ResultDto.Failure(new[] { "Đã có manager đang hoạt động. Vui lòng khóa manager hiện tại trước khi mở khóa manager này." });
        }

        user.Status = "Active";
        user.UpdatedAt = DateTime.UtcNow;
        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
            return ResultDto.Failure(updateResult.Errors.Select(e => e.Description));

        await _userManager.SetLockoutEndDateAsync(user, null);

        return ResultDto.Success();
    }

    public async Task<ResultDto> CreateStaffAsync(CreateUserDto dto, int? poolId = null, CancellationToken cancellationToken = default)
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

        if (poolId.HasValue)
        {
            var poolExists = await _readContext.Pools.AnyAsync(p => p.PoolId == poolId.Value, cancellationToken);
            if (!poolExists)
                return ResultDto.Failure(new[] { "Bể bơi không tồn tại." });

            var alreadyAssigned = await _readContext.PoolStaffAssignments
                .AnyAsync(a => a.PoolId == poolId.Value, cancellationToken);
            if (alreadyAssigned)
                return ResultDto.Failure(new[] { "Bể bơi này đã có nhân viên phụ trách." });

            var assignment = new PoolStaffAssignment
            {
                PoolId = poolId.Value,
                StaffId = user.Id,
                AssignedAt = DateTime.UtcNow
            };

            _writeContext.PoolStaffAssignments.Add(assignment);
            await _writeContext.SaveChangesAsync(cancellationToken);
        }

        return ResultDto.Success();
    }

    public async Task<ResultDto> CreateManagerAsync(CreateUserDto dto, CancellationToken cancellationToken = default)
    {
        var existingActiveManager = await _readContext.Users
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
                x => x.ur!.RoleId,
                r => r.Id,
                (x, roles) => new { x.u, roles }
            )
            .SelectMany(
                x => x.roles.DefaultIfEmpty(),
                (x, r) => new { x.u, RoleName = r!.Name }
            )
            .FirstOrDefaultAsync(x => x.RoleName == "Manager" && x.u.Status == "Active", cancellationToken);

        if (existingActiveManager != null)
            return ResultDto.Failure(new[] { "Manager hiện tại đang hoạt động. Vui lòng khóa manager cũ trước khi tạo mới." });

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

    public async Task<ResultDto> UpdateUserAsync(Guid userId, UpdateUserDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return ResultDto.Failure(new[] { "Người dùng không tồn tại." });

        user.UserName = dto.UserName;
        user.Email = dto.Email;
        user.FullName = dto.FullName;
        user.PhoneNumber = dto.PhoneNumber;
        user.Address = dto.Address;
        user.Gender = dto.Gender;
        user.Dob = dto.Dob;
        user.UpdatedAt = DateTime.UtcNow;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            return ResultDto.Failure(updateResult.Errors.Select(e => e.Description));

        // Handle pool assignment change
        var currentAssignment = await _writeContext.PoolStaffAssignments
            .FirstOrDefaultAsync(a => a.StaffId == userId, cancellationToken);

        if (dto.PoolId.HasValue)
        {
            var poolExists = await _readContext.Pools.AnyAsync(p => p.PoolId == dto.PoolId.Value, cancellationToken);
            if (!poolExists)
                return ResultDto.Failure(new[] { "Bể bơi không tồn tại." });

            if (currentAssignment != null)
            {
                if (currentAssignment.PoolId != dto.PoolId.Value)
                {
                    var alreadyAssigned = await _readContext.PoolStaffAssignments
                        .AnyAsync(a => a.PoolId == dto.PoolId.Value && a.StaffId != userId, cancellationToken);
                    if (alreadyAssigned)
                        return ResultDto.Failure(new[] { "Bể bơi này đã có nhân viên khác phụ trách." });

                    currentAssignment.PoolId = dto.PoolId.Value;
                    await _writeContext.SaveChangesAsync(cancellationToken);
                }
            }
            else
            {
                var alreadyAssigned = await _readContext.PoolStaffAssignments
                    .AnyAsync(a => a.PoolId == dto.PoolId.Value, cancellationToken);
                if (alreadyAssigned)
                    return ResultDto.Failure(new[] { "Bể bơi này đã có nhân viên phụ trách." });

                var assignment = new PoolStaffAssignment
                {
                    PoolId = dto.PoolId.Value,
                    StaffId = userId,
                    AssignedAt = DateTime.UtcNow
                };
                _writeContext.PoolStaffAssignments.Add(assignment);
                await _writeContext.SaveChangesAsync(cancellationToken);
            }
        }
        else
        {
            if (currentAssignment != null)
            {
                _writeContext.PoolStaffAssignments.Remove(currentAssignment);
                await _writeContext.SaveChangesAsync(cancellationToken);
            }
        }

        return ResultDto.Success();
    }

    public async Task<List<AdminPoolDto>> GetPoolsAsync(CancellationToken cancellationToken = default)
    {
        return await _readContext.Pools
            .Where(p => p.Status == "Active")
            .OrderBy(p => p.PoolName)
            .Select(p => new AdminPoolDto
            {
                PoolId = p.PoolId,
                PoolName = p.PoolName,
                Address = p.Address
            })
            .ToListAsync(cancellationToken);
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

        var paidBookingStatuses = new[] { "Paid", "CheckIn", "Completed" };

        var totalRevenue = await _readContext.Bookings
            .Where(b => paidBookingStatuses.Contains(b.Status) || (b.Payment != null && (b.Payment.Status == "Success" || b.Payment.Status == "Completed")))
            .SumAsync(b => (decimal?)b.TotalAmount, cancellationToken) ?? 0;

        var totalUsers = await _readContext.Users.CountAsync(cancellationToken);
        var totalBookings = await _readContext.Bookings.CountAsync(cancellationToken);
        var totalPools = await _readContext.Pools.CountAsync(cancellationToken);

        var todayBookings = await _readContext.Bookings
            .CountAsync(b => b.BookingDate == today, cancellationToken);

        var thisMonthRevenue = await _readContext.Bookings
            .Where(b => (paidBookingStatuses.Contains(b.Status) || (b.Payment != null && (b.Payment.Status == "Success" || b.Payment.Status == "Completed"))) && b.CreatedAt >= startOfMonth)
            .SumAsync(b => (decimal?)b.TotalAmount, cancellationToken) ?? 0;

        var newUsersThisMonth = await _readContext.Users
            .CountAsync(u => u.CreatedAt >= startOfMonth, cancellationToken);

        var monthlyRevenues = await _readContext.Bookings
            .Where(b => (paidBookingStatuses.Contains(b.Status) || (b.Payment != null && (b.Payment.Status == "Success" || b.Payment.Status == "Completed"))) && b.CreatedAt.Year == now.Year)
            .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
            .Select(g => new MonthlyRevenueDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Revenue = g.Sum(b => b.TotalAmount)
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
                x => x.ur!.RoleId,
                r => r.Id,
                (x, roles) => new { x.u, roles }
            )
            .SelectMany(
                x => x.roles.DefaultIfEmpty(),
                (x, r) => new { x.u, r }
            )
            .GroupBy(x => x.r!.Name ?? "Customer")
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

    public async Task<PagedResultDto<ContactRequestListDto>> GetContactRequestsAsync(int page, int pageSize, string? status, CancellationToken cancellationToken = default)
    {
        var query = _readContext.ContactRequests.AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(c => c.Status == status);

        var totalCount = await query.CountAsync(cancellationToken);

        var contacts = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = contacts.Select(c => new ContactRequestListDto
        {
            ContactRequestId = c.ContactRequestId,
            FullName = c.FullName,
            Email = c.Email,
            PhoneNumber = c.PhoneNumber,
            Category = c.Category,
            Message = c.Message,
            Status = c.Status,
            CreatedAt = c.CreatedAt
        }).ToList();

        return new PagedResultDto<ContactRequestListDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ResultDto> RespondContactRequestAsync(int contactRequestId, string responseMessage, CancellationToken cancellationToken = default)
    {
        var adminIdString = _currentUserService.UserId;
        if (string.IsNullOrEmpty(adminIdString) || !Guid.TryParse(adminIdString, out var adminId))
            return ResultDto.Failure(new[] { "Admin chưa đăng nhập hoặc không hợp lệ." });

        var contact = await _writeContext.ContactRequests
            .FirstOrDefaultAsync(c => c.ContactRequestId == contactRequestId, cancellationToken);

        if (contact is null)
            return ResultDto.Failure(new[] { "Không tìm thấy yêu cầu hỗ trợ." });

        if (contact.Status != "Pending")
            return ResultDto.Failure(new[] { $"Yêu cầu hỗ trợ này đã được xử lý (Trạng thái: {contact.Status})." });

        contact.Status = "Resolved";
        contact.HandledByUserId = adminId;
        contact.HandledAt = DateTime.UtcNow;

        await _writeContext.SaveChangesAsync(cancellationToken);

        try
        {
            var emailSubject = $"Phản hồi yêu cầu hỗ trợ: {contact.Category}";
            var emailBody = $@"
                    <p>Chào {contact.FullName},</p>
                    <p>Ban quản lý SBS đã phản hồi yêu cầu hỗ trợ của bạn như sau:</p>
                    <hr/>
                    <p>{responseMessage}</p>
                    <hr/>
                    <p>Nếu bạn còn thắc mắc, vui lòng liên hệ lại với chúng tôi qua email này.</p>
                    <p>Trân trọng,<br>Ban quản lý SBS</p>";

            await _emailService.SendEmailAsync(contact.Email, emailSubject, emailBody);
        }
        catch
        {
        }

        return ResultDto.Success();
    }
}
