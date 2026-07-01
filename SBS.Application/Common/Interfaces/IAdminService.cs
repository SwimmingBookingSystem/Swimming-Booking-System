using SBS.Application.Common.Dtos.Admin;
using SBS.Application.Common.Dtos.Profile;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Common.Interfaces;

public interface IAdminService
{
    Task<List<UserListDto>> GetUsersAsync(CancellationToken cancellationToken = default);

    Task<ResultDto> LockUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<ResultDto> UnlockUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<ResultDto> CreateStaffAsync(CreateUserDto dto, int? poolId = null, CancellationToken cancellationToken = default);

    Task<ResultDto> CreateManagerAsync(CreateUserDto dto, CancellationToken cancellationToken = default);

    Task<ResultDto> UpdateUserAsync(Guid userId, UpdateUserDto dto, CancellationToken cancellationToken = default);

    Task<List<AdminPoolDto>> GetPoolsAsync(CancellationToken cancellationToken = default);

    Task<List<RoleDto>> GetRolesAsync(CancellationToken cancellationToken = default);

    Task<DashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default);
}
