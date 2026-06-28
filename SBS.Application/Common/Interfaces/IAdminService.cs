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

    Task<ResultDto> CreateStaffAsync(CreateUserDto dto, CancellationToken cancellationToken = default);

    Task<ResultDto> CreateManagerAsync(CreateUserDto dto, CancellationToken cancellationToken = default);

    Task<ResultDto> ChangeUserRoleAsync(Guid userId, string newRole, CancellationToken cancellationToken = default);

    Task<List<RoleDto>> GetRolesAsync(CancellationToken cancellationToken = default);

    Task<DashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default);
}
