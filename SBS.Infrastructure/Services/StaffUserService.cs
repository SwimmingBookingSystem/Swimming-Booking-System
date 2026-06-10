using Microsoft.EntityFrameworkCore;
using SBS.Application.Common.Dtos.Staff;
using SBS.Application.Common.Interfaces;
using SBS.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Infrastructure.Services;

/// <summary>
/// Implementation của IStaffUserService — hoàn toàn độc lập với IdentityService.
/// Chỉ phục vụ các nghiệp vụ của Staff, dùng ReadDbContext (read-only, hiệu năng cao).
/// </summary>
public class StaffUserService : IStaffUserService
{
    private readonly ReadDbContext _readContext;

    public StaffUserService(ReadDbContext readContext)
    {
        _readContext = readContext;
    }

    /// <inheritdoc/>
    public async Task<UserBriefDto?> GetUserBriefAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _readContext.Users
            .Where(u => u.Id == userId)
            .Select(u => new UserBriefDto
            {
                UserId = u.Id,
                FullName = u.FullName ?? u.UserName ?? "Khách vãng lai",
                Email = u.Email ?? string.Empty,
                PhoneNumber = u.PhoneNumber
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<Guid>> FindUserIdsByPhoneOrEmailAsync(
        string? phone,
        string? email,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(phone) && string.IsNullOrWhiteSpace(email))
            return new List<Guid>();

        var query = _readContext.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(phone) && !string.IsNullOrWhiteSpace(email))
            query = query.Where(u => u.PhoneNumber == phone || u.Email == email);
        else if (!string.IsNullOrWhiteSpace(phone))
            query = query.Where(u => u.PhoneNumber == phone);
        else
            query = query.Where(u => u.Email == email);

        return await query.Select(u => u.Id).ToListAsync(cancellationToken);
    }
}
