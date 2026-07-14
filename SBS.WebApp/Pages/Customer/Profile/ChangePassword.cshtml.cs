using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using SBS.WebApp.Models.Profile;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace SBS.WebApp.Pages.Customer.Profile;

[Authorize(Roles = "Customer")]
public class ChangePasswordModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public ChangePasswordModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [BindProperty]
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại")]
    public string CurrentPassword { get; set; } = null!;

    [BindProperty]
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
    [MinLength(1, ErrorMessage = "Mật khẩu mới phải có ít nhất 1 ký tự")] // Theo setting Identity hiện tại
    public string NewPassword { get; set; } = null!;

    [BindProperty]
    [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới")]
    [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
    public string ConfirmPassword { get; set; } = null!;

    public void OnGet()
    {
    }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(_configuration["ApiBaseUrl"] ?? "https://localhost:7179");
        
        if (Request.Cookies.TryGetValue("accessToken", out var token))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        
        return client;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var client = CreateClient();
        var request = new ChangePasswordRequest
        {
            CurrentPassword = CurrentPassword,
            NewPassword = NewPassword,
            ConfirmPassword = ConfirmPassword
        };

        var response = await client.PostAsJsonAsync("/api/profile/change-password", request);

        if (response.IsSuccessStatusCode)
        {
            TempData["SuccessMessage"] = "Thay đổi mật khẩu thành công!";
            return RedirectToPage();
        }
        else
        {
            try 
            {
                var errorResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
                if (errorResponse.TryGetProperty("errors", out var errorsObj) && errorsObj.ValueKind == JsonValueKind.Array)
                {
                    foreach (var err in errorsObj.EnumerateArray())
                    {
                        ModelState.AddModelError(string.Empty, err.GetString() ?? "Lỗi không xác định");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Mật khẩu hiện tại không đúng hoặc có lỗi xảy ra.");
                }
            } 
            catch 
            {
                ModelState.AddModelError(string.Empty, "Mật khẩu hiện tại không đúng hoặc có lỗi xảy ra.");
            }
            return Page();
        }
    }
}
