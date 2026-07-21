using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SBS.WebApp.Pages.Auth;

public class LogoutModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public LogoutModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        await PerformLogoutAsync();
        return Redirect("/Index");
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await PerformLogoutAsync();
        return Redirect("/Index");
    }

    private async Task PerformLogoutAsync()
    {
        try
        {
            // Lấy refresh token từ claim hiện tại để gửi yêu cầu thu hồi phía API
            var refreshToken = User.FindFirst("RefreshToken")?.Value;
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var client = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiBaseUrl"] ?? "https://localhost:7179";

                // Đính kèm refresh token dạng Cookie hoặc body nếu cần
                var request = new HttpRequestMessage(HttpMethod.Post, $"{apiBaseUrl}/api/Auth/logout");
                
                // Đóng gói Cookie refreshToken gửi sang API
                request.Headers.Add("Cookie", $"refreshToken={refreshToken}");

                await client.SendAsync(request);
            }
        }
        catch (Exception)
        {
            // Bỏ qua lỗi kết nối API khi đăng xuất để đảm bảo luôn dọn dẹp được session client
        }

        // Xóa cookie xác thực mã hóa của WebApp (.AspNetCore.Cookies) ở trình duyệt
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
