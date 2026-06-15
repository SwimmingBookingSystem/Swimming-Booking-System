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
}
