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
}
