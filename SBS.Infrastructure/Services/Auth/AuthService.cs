using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SBS.Application.Common.Dtos.Auth;
using SBS.Application.Common.Interfaces;
using SBS.Domain.Entities;
using SBS.Infrastructure.Data;
using SBS.Infrastructure.Identity;
using System;
using System.Linq;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Threading;
using System.Threading.Tasks;

namespace SBS.Infrastructure.Services.Auth;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<AppUser> userManager,
        ApplicationDbContext context,
        ITokenService tokenService,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _context = context;
        _tokenService = tokenService;
        _configuration = configuration;
    }

    public async Task<AuthResultDto> LoginAsync(string userName, string password, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
        {
            return AuthResultDto.Failure(new[] { "Tài khoản hoặc mật khẩu không chính xác." });
        }

        if (user.Status != "Active")
        {
            return AuthResultDto.Failure(new[] { "Tài khoản đang bị khóa hoặc ngừng hoạt động." });
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
        if (!isPasswordValid)
        {
            return AuthResultDto.Failure(new[] { "Tài khoản hoặc mật khẩu không chính xác." });
        }

        var roles = await _userManager.GetRolesAsync(user);
        var (accessToken, jwtId) = _tokenService.GenerateAccessToken(user.Id, user.UserName ?? string.Empty, user.Email ?? string.Empty, roles);
        var refreshTokenValue = _tokenService.GenerateRefreshToken();

        var expiryDaysStr = _configuration["JwtSettings:RefreshTokenExpirationInDays"] ?? "7";
        if (!double.TryParse(expiryDaysStr, out var expiryDays))
        {
            expiryDays = 7;
        }
        var expiryDate = DateTime.UtcNow.AddDays(expiryDays);

        var refreshToken = new RefreshToken
        {
            Token = refreshTokenValue,
            JwtId = jwtId,
            UserId = user.Id,
            ExpiryDate = expiryDate,
            IsUsed = false,
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        return AuthResultDto.Success(new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            ExpiryDate = expiryDate,
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            FullName = user.FullName,
            Role = roles.Count > 0 ? roles[0] : string.Empty
        });
    }

    public async Task<AuthResultDto> RefreshTokenAsync(string accessToken, string refreshToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken);
            if (principal == null)
            {
                return AuthResultDto.Failure(new[] { "Access Token không hợp lệ." });
            }

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier) ?? principal.FindFirst("sub");
            var jwtIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Jti) ?? principal.FindFirst("jti") ?? principal.Claims.FirstOrDefault(c => c.Type.EndsWith("jwtid") || c.Type.EndsWith("jti"));

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                var claimsList = string.Join(", ", principal.Claims.Select(c => $"{c.Type}={c.Value}"));
                return AuthResultDto.Failure(new[] { $"Access Token không hợp lệ. Claims: {claimsList}" });
            }

            var savedRefreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == refreshToken, cancellationToken);

            if (savedRefreshToken == null)
            {
                return AuthResultDto.Failure(new[] { "Refresh Token không tồn tại." });
            }

            if (savedRefreshToken.IsUsed)
            {
                return AuthResultDto.Failure(new[] { "Refresh Token này đã được sử dụng." });
            }

            if (savedRefreshToken.IsRevoked)
            {
                return AuthResultDto.Failure(new[] { "Refresh Token này đã bị thu hồi." });
            }

            if (savedRefreshToken.ExpiryDate <= DateTime.UtcNow)
            {
                return AuthResultDto.Failure(new[] { "Refresh Token này đã hết hạn." });
            }

            if (savedRefreshToken.UserId != userId)
            {
                return AuthResultDto.Failure(new[] { "Token không khớp với người dùng." });
            }

            // Chỉ so sánh JwtId nếu token có chứa claim này
            if (jwtIdClaim != null && savedRefreshToken.JwtId != jwtIdClaim.Value)
            {
                return AuthResultDto.Failure(new[] { "Mã token không trùng khớp." });
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null || user.Status != "Active")
            {
                return AuthResultDto.Failure(new[] { "Người dùng không tồn tại hoặc đã bị khóa." });
            }

            var roles = await _userManager.GetRolesAsync(user);

            var (newAccessToken, newJwtId) = _tokenService.GenerateAccessToken(user.Id, user.UserName ?? string.Empty, user.Email ?? string.Empty, roles);
            var newRefreshTokenValue = _tokenService.GenerateRefreshToken();

            var expiryDaysStr = _configuration["JwtSettings:RefreshTokenExpirationInDays"] ?? "7";
            if (!double.TryParse(expiryDaysStr, out var expiryDays))
            {
                expiryDays = 7;
            }
            var newExpiryDate = DateTime.UtcNow.AddDays(expiryDays);

            savedRefreshToken.IsUsed = true;
            _context.RefreshTokens.Update(savedRefreshToken);

            var newRefreshToken = new RefreshToken
            {
                Token = newRefreshTokenValue,
                JwtId = newJwtId,
                UserId = user.Id,
                ExpiryDate = newExpiryDate,
                IsUsed = false,
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(newRefreshToken);
            await _context.SaveChangesAsync(cancellationToken);

            return AuthResultDto.Success(new AuthResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshTokenValue,
                ExpiryDate = newExpiryDate,
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                FullName = user.FullName,
                Role = roles.Count > 0 ? roles[0] : string.Empty
            });
        }
        catch (Exception)
        {
            return AuthResultDto.Failure(new[] { "Đã xảy ra lỗi trong quá trình xử lý token." });
        }
    }
}
