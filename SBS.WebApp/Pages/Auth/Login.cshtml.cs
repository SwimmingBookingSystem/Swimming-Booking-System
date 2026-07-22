using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SBS.WebApp.Pages.Auth;

public class LoginModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    [BindProperty]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public string ErrorMessage { get; set; } = string.Empty;

    public LoginModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public IActionResult OnGet()
    {
        // Nếu đã đăng nhập rồi, tự động chuyển hướng theo vai trò
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            return RedirectToRolePage(User.FindFirst(ClaimTypes.Role)?.Value ?? "");
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
        {
            ErrorMessage = "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.";
            return Page();
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            var apiBaseUrl = _configuration["ApiBaseUrl"] ?? "https://localhost:7179";

            var response = await client.PostAsJsonAsync($"{apiBaseUrl}/api/Auth/login", new
            {
                userName = Username,
                password = Password
            });

            if (!response.IsSuccessStatusCode)
            {
                var errorData = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                ErrorMessage = errorData?.Message ?? "Tên đăng nhập hoặc mật khẩu không chính xác.";
                return Page();
            }

            var apiResult = await response.Content.ReadFromJsonAsync<ApiLoginResponse>();
            if (apiResult == null || string.IsNullOrEmpty(apiResult.Role))
            {
                ErrorMessage = "Không thể lấy thông tin xác thực từ hệ thống.";
                return Page();
            }

            // Đăng nhập thành công -> Tạo các claims của phiên làm việc
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, apiResult.Id.ToString()),
                new Claim(ClaimTypes.Name, apiResult.UserName ?? Username),
                new Claim("FullName", apiResult.FullName ?? ""),
                new Claim(ClaimTypes.Role, apiResult.Role),
                new Claim("AccessToken", apiResult.AccessToken ?? ""),
                new Claim("RefreshToken", apiResult.RefreshToken ?? ""),
                new Claim("PoolName", apiResult.PoolName ?? "")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = apiResult.ExpiryDate > DateTime.UtcNow ? apiResult.ExpiryDate : DateTime.UtcNow.AddMinutes(30)
            };

            // Tiến hành ghi Cookie mã hóa (.AspNetCore.Cookies) xuống Browser của người dùng
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return RedirectToRolePage(apiResult.Role);
        }
        catch (Exception ex)
        {
            ErrorMessage = "Đã xảy ra lỗi khi kết nối với máy chủ: " + ex.Message;
            return Page();
        }
    }

    private IActionResult RedirectToRolePage(string role)
    {
        return role.ToLower() switch
        {
            "admin" => Redirect("/Admin/Dashboard"),
            "manager" => Redirect("/Manager/Dashboard"),
            "staff" => Redirect("/Staff/CheckIn"),
            "customer" => Redirect("/"),
            _ => Redirect("/Index")
        };
    }

    private class ApiErrorResponse
    {
        public string Message { get; set; } = string.Empty;
    }

    private class ApiLoginResponse
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string PoolName { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
    }
}
