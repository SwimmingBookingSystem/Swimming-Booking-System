using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using SBS.Application.Common.Dtos.Auth;
using SBS.Application.Common.Interfaces;
using SBS.Domain.Entities;
using SBS.Infrastructure.Data;
using SBS.Infrastructure.Identity;
using System;
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
}
