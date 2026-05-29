namespace SBS.Application.Common.Interfaces;

/// <summary>
/// Service for accessing ASP.NET Core Identity user information.
/// Implemented in SBS.Infrastructure to avoid circular dependency.
/// </summary>
public interface IIdentityService
{
    Task<string?> GetUserFullNameAsync(int userId, CancellationToken cancellationToken = default);
    Task<string?> GetUserPhoneAsync(int userId, CancellationToken cancellationToken = default);
}
