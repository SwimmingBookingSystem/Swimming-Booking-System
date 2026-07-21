using SBS.Application.Common.Dtos;
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

    Task<ResultDto> RespondContactRequestAsync(int contactRequestId, string responseMessage, CancellationToken cancellationToken = default);

    Task<PagedResultDto<ContactRequestListDto>> GetContactRequestsAsync(int page, int pageSize, string? status, CancellationToken cancellationToken = default);

    Task<PagedResultDto<BookingListDto>> GetBookingsAsync(int page, int pageSize, string? status, string? search, string? fromDate, string? toDate, CancellationToken cancellationToken = default);
}
