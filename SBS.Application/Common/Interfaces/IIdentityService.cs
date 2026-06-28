using SBS.Application.Common.Dtos.Profile;
using SBS.Application.Common.Dtos.Auth;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<UserProfileDto?> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default);
    
    Task<bool> UpdateProfileAsync(Guid userId, UpdateProfileDto dto, CancellationToken cancellationToken = default);
    
    Task<bool> UpdateAvatarAsync(Guid userId, string avatarUrl, CancellationToken cancellationToken = default);
    
    Task<ResultDto> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword, CancellationToken cancellationToken = default);
}
