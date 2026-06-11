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
            return _httpContextAccessor.HttpContext?.User?
                .FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}
