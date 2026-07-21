using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SBS.Application.Common.Dtos.Auth;
using SBS.Application.Common.Dtos.Profile;
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
    private readonly IEmailService _emailService;
    private readonly IDistributedCache _cache;

    public AuthService(
        UserManager<AppUser> userManager,
        ApplicationDbContext context,
        ITokenService tokenService,
        IConfiguration configuration,
        IEmailService emailService,
        IDistributedCache cache)
    {
        _userManager = userManager;
        _context = context;
        _tokenService = tokenService;
        _configuration = configuration;
        _emailService = emailService;
        _cache = cache;
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

        if (!user.EmailConfirmed)
        {
            return AuthResultDto.Failure(new[] { "Tài khoản chưa được xác thực email. Vui lòng xác thực bằng mã OTP trước khi đăng nhập." });
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

        var cacheKey = $"refreshToken:{refreshTokenValue}";
        var cacheValue = JsonConvert.SerializeObject(refreshToken);
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpiration = expiryDate
        };
        await _cache.SetStringAsync(cacheKey, cacheValue, cacheOptions, cancellationToken);

        string? poolName = null;
        if (roles.Contains("Staff"))
        {
            var assignment = await _context.PoolStaffAssignments
                .Include(a => a.Pool)
                .FirstOrDefaultAsync(a => a.StaffId == user.Id, cancellationToken);
            if (assignment?.Pool != null)
            {
                poolName = assignment.Pool.PoolName;
            }
        }

        return AuthResultDto.Success(new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            ExpiryDate = expiryDate,
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            FullName = user.FullName,
            Role = roles.Count > 0 ? roles[0] : string.Empty,
            PoolName = poolName
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

            var cacheKey = $"refreshToken:{refreshToken}";
            var cachedData = await _cache.GetStringAsync(cacheKey, cancellationToken);

            if (string.IsNullOrEmpty(cachedData))
            {
                return AuthResultDto.Failure(new[] { "Refresh Token không tồn tại hoặc đã hết hạn." });
            }

            var savedRefreshToken = JsonConvert.DeserializeObject<RefreshToken>(cachedData);

            if (savedRefreshToken == null)
            {
                return AuthResultDto.Failure(new[] { "Refresh Token không hợp lệ." });
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

            // Đánh dấu token cũ đã được sử dụng và lưu lại vào Redis với thời gian hết hạn còn lại
            savedRefreshToken.IsUsed = true;
            var remainingTime = savedRefreshToken.ExpiryDate - DateTime.UtcNow;
            if (remainingTime > TimeSpan.Zero)
            {
                var oldCacheValue = JsonConvert.SerializeObject(savedRefreshToken);
                var oldCacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = savedRefreshToken.ExpiryDate
                };
                await _cache.SetStringAsync(cacheKey, oldCacheValue, oldCacheOptions, cancellationToken);
            }

            // Lưu token mới vào Redis
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

            var newCacheKey = $"refreshToken:{newRefreshTokenValue}";
            var newCacheValue = JsonConvert.SerializeObject(newRefreshToken);
            var newCacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = newExpiryDate
            };
            await _cache.SetStringAsync(newCacheKey, newCacheValue, newCacheOptions, cancellationToken);

            string? poolName = null;
            if (roles.Contains("Staff"))
            {
                var assignment = await _context.PoolStaffAssignments
                    .Include(a => a.Pool)
                    .FirstOrDefaultAsync(a => a.StaffId == user.Id, cancellationToken);
                if (assignment?.Pool != null)
                {
                    poolName = assignment.Pool.PoolName;
                }
            }

            return AuthResultDto.Success(new AuthResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshTokenValue,
                ExpiryDate = newExpiryDate,
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                FullName = user.FullName,
                Role = roles.Count > 0 ? roles[0] : string.Empty,
                PoolName = poolName
            });
        }
        catch (Exception)
        {
            return AuthResultDto.Failure(new[] { "Đã xảy ra lỗi trong quá trình xử lý token." });
        }
    }

    public async Task<ResultDto> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"refreshToken:{refreshToken}";
        await _cache.RemoveAsync(cacheKey, cancellationToken);
        return ResultDto.Success();
    }

    public async Task<ResultDto> RegisterAsync(
        string userName,
        string email,
        string password,
        string fullName,
        string? phoneNumber,
        DateOnly? dob,
        string? address,
        CancellationToken cancellationToken = default)
    {
        // Check duplicate Email
        var existingUserByEmail = await _userManager.FindByEmailAsync(email);
        if (existingUserByEmail != null)
        {
            // If the existing user has NOT confirmed their email, we can delete it and let the registration recreate it
            if (!existingUserByEmail.EmailConfirmed)
            {
                await _userManager.DeleteAsync(existingUserByEmail);
            }
            else
            {
                return ResultDto.Failure(new[] { "Email đã được sử dụng." });
            }
        }

        // Check duplicate PhoneNumber
        if (!string.IsNullOrEmpty(phoneNumber))
        {
            var existingUserByPhone = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber, cancellationToken);
            if (existingUserByPhone != null)
            {
                // If the existing user has NOT confirmed their email, we can delete it and let the registration recreate it
                if (!existingUserByPhone.EmailConfirmed)
                {
                    await _userManager.DeleteAsync(existingUserByPhone);
                }
                else
                {
                    return ResultDto.Failure(new[] { "Số điện thoại đã được sử dụng." });
                }
            }
        }

        // Create the user object
        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = userName,
            Email = email,
            FullName = fullName,
            PhoneNumber = phoneNumber,
            Address = address,
            Dob = dob,
            Gender = null,
            Status = "Active",
            EmailConfirmed = false, // Must verify OTP to set to true
            CreatedAt = DateTime.UtcNow
        };

        // This will run all default Identity validations (e.g. Password Policy) immediately at registration!
        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            return ResultDto.Failure(errors);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, "Customer");
        if (!roleResult.Succeeded)
        {
            var errors = roleResult.Errors.Select(e => e.Description);
            return ResultDto.Failure(errors);
        }

        // Generate OTP
        try
        {
            var otp = await _userManager.GenerateUserTokenAsync(user, TokenOptions.DefaultEmailProvider, "EmailConfirmation");
            await _emailService.SendEmailAsync(email, "Xác thực tài khoản - Swimming Booking System", 
                $"Chào {fullName},<br/><br/>Cảm ơn bạn đã đăng ký tài khoản tại Swimming Booking System. " +
                $"Mã OTP xác thực tài khoản của bạn là: <strong>{otp}</strong>. Mã này có hiệu lực trong vòng 5 phút.<br/><br/>" +
                $"Nếu bạn không yêu cầu đăng ký này, vui lòng bỏ qua email này.");
        }
        catch (Exception)
        {
            // Logged/handled inside EmailService
        }

        return ResultDto.Success();
    }

    public async Task<ResultDto> VerifyOtpAsync(string email, string otp, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return ResultDto.Failure(new[] { "Không tìm thấy thông tin tài khoản." });
        }

        if (user.EmailConfirmed)
        {
            return ResultDto.Failure(new[] { "Email đã được xác thực trước đó." });
        }

        var isValid = await _userManager.VerifyUserTokenAsync(user, TokenOptions.DefaultEmailProvider, "EmailConfirmation", otp);
        if (!isValid)
        {
            return ResultDto.Failure(new[] { "Mã OTP không đúng hoặc đã hết hạn." });
        }

        user.EmailConfirmed = true;
        user.Status = "Active";
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = updateResult.Errors.Select(e => e.Description);
            return ResultDto.Failure(errors);
        }

        return ResultDto.Success();
    }

    public async Task<ResultDto> ResendOtpAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return ResultDto.Failure(new[] { "Không tìm thấy thông tin tài khoản." });
        }

        if (user.EmailConfirmed)
        {
            return ResultDto.Failure(new[] { "Email đã được xác thực trước đó." });
        }

        // Invalidate any older tokens for security
        await _userManager.UpdateSecurityStampAsync(user);

        var otp = await _userManager.GenerateUserTokenAsync(user, TokenOptions.DefaultEmailProvider, "EmailConfirmation");
        
        try
        {
            await _emailService.SendEmailAsync(email, "Gửi lại mã OTP - Swimming Booking System", 
                $"Chào {user.FullName},<br/><br/>Yêu cầu gửi lại mã OTP xác thực tài khoản của bạn đã được thực hiện. " +
                $"Mã OTP mới là: <strong>{otp}</strong>. Mã này có hiệu lực trong vòng 5 phút.<br/><br/>" +
                $"Nếu bạn không yêu cầu mã này, vui lòng bảo mật tài khoản của mình.");
        }
        catch (Exception)
        {
            // Logged/handled inside EmailService
        }

        return ResultDto.Success();
    }

    public async Task<ResultDto> ForgotPasswordAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return ResultDto.Failure(new[] { "Không tìm thấy thông tin tài khoản." });
        }

        // Generate OTP for Password Reset
        var otp = await _userManager.GenerateUserTokenAsync(user, TokenOptions.DefaultEmailProvider, "ResetPassword");

        try
        {
            await _emailService.SendEmailAsync(email, "Yêu cầu cấp lại mật khẩu - Swimming Booking System", 
                $"Chào {user.FullName},<br/><br/>Chúng tôi nhận được yêu cầu khôi phục mật khẩu từ bạn. " +
                $"Mã OTP để đặt lại mật khẩu của bạn là: <strong>{otp}</strong>. Mã này có hiệu lực trong vòng 5 phút.<br/><br/>" +
                $"Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email.");
        }
        catch (Exception)
        {
            // Logged/handled inside EmailService
        }

        return ResultDto.Success();
    }

    public async Task<ResultDto> VerifyResetOtpAsync(string email, string otp, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return ResultDto.Failure(new[] { "Không tìm thấy thông tin tài khoản." });
        }

        var isValid = await _userManager.VerifyUserTokenAsync(user, TokenOptions.DefaultEmailProvider, "ResetPassword", otp);
        if (!isValid)
        {
            return ResultDto.Failure(new[] { "Mã OTP không đúng hoặc đã hết hạn." });
        }

        return ResultDto.Success();
    }

    public async Task<ResultDto> ResetPasswordAsync(string email, string otp, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return ResultDto.Failure(new[] { "Không tìm thấy thông tin tài khoản." });
        }

        // Verify OTP
        var isValid = await _userManager.VerifyUserTokenAsync(user, TokenOptions.DefaultEmailProvider, "ResetPassword", otp);
        if (!isValid)
        {
            return ResultDto.Failure(new[] { "Mã OTP không đúng hoặc đã hết hạn." });
        }

        // Generate reset token internally
        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        
        // Reset password
        var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            return ResultDto.Failure(errors);
        }

        return ResultDto.Success();
    }
}
