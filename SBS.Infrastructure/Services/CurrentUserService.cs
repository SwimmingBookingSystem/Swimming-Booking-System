using Microsoft.AspNetCore.Http;
using SBS.Application.Common.Interfaces;
using SBS.Infrastructure.Data;
using System.Linq;
using System.Security.Claims;

namespace SBS.Infrastructure.Services;


// Dịch vụ lấy thông tin người dùng đang đăng nhập hiện tại.
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ApplicationDbContext _context;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, ApplicationDbContext context)
    {
        _httpContextAccessor = httpContextAccessor;
        _context = context;
    }

    public string? UserId
    {
        get
        {
            // 1. Lấy UserId từ JWT Claim nếu người dùng đã đăng nhập (khi đã hoàn thành chức năng Login)
            var userIdFromClaim = _httpContextAccessor.HttpContext?.User?
                .FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(userIdFromClaim))
            {
                return userIdFromClaim;
            }

            // 2. Fallback (Mock): Trả về ID của tài khoản customer1 đã seed trong DB phục vụ việc kiểm thử
            return _context.Users
                .FirstOrDefault(u => u.UserName == "customer1")?.Id.ToString();
        }
    }
}
