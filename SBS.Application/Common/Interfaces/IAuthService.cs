using SBS.Application.Common.Dtos.Auth;
using SBS.Application.Common.Dtos.Profile;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Application.Common.Interfaces;

public interface IAuthService
{
    Task<AuthResultDto> LoginAsync(string userName, string password, CancellationToken cancellationToken = default);
    Task<AuthResultDto> RefreshTokenAsync(string accessToken, string refreshToken, CancellationToken cancellationToken = default);
    Task<ResultDto> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<ResultDto> RegisterAsync(
        string userName,
        string email,
        string password,
        string fullName,
        string? phoneNumber,
        DateOnly? dob,
        string? address,
        CancellationToken cancellationToken = default);
    Task<ResultDto> VerifyOtpAsync(string email, string otp, CancellationToken cancellationToken = default);
    Task<ResultDto> ResendOtpAsync(string email, CancellationToken cancellationToken = default);
    Task<ResultDto> ForgotPasswordAsync(string email, CancellationToken cancellationToken = default);
    Task<ResultDto> ResetPasswordAsync(string email, string otp, string newPassword, CancellationToken cancellationToken = default);
}
