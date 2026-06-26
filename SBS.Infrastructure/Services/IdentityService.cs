using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SBS.Application.Common.Dtos.Profile;
using SBS.Application.Common.Dtos.Auth;
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

public class IdentityService : IIdentityService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly ReadDbContext _readContext;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;

    public IdentityService(
        UserManager<AppUser> userManager,
        ApplicationDbContext context,
        ReadDbContext readContext,
        ITokenService tokenService,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _context = context;
        _readContext = readContext;
        _tokenService = tokenService;
        _configuration = configuration;
    }

    public async Task<UserProfileDto?> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _readContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            return null;
        }

        return new UserProfileDto
        {
            UserId = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address,
            Dob = user.Dob,
            Gender = user.Gender,
            Images = user.AvatarUrl,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<bool> UpdateProfileAsync(Guid userId, UpdateProfileDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return false;
        }

        // Cập nhật thông tin
        user.FullName = dto.FullName;
        user.PhoneNumber = dto.PhoneNumber;
        user.Address = dto.Address;
        user.Dob = dto.Dob;
        user.Gender = dto.Gender;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> UpdateAvatarAsync(Guid userId, string avatarUrl, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return false;
        }

        user.AvatarUrl = avatarUrl;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<ResultDto> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return ResultDto.Failure(new[] { "Người dùng không tồn tại." });
        }

        var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
        if (result.Succeeded)
        {
            return ResultDto.Success();
        }

        // Trích xuất các lỗi từ Identity và map sang DTO
        var errors = result.Errors.Select(e => e.Description);
        return ResultDto.Failure(errors);
    }
}
