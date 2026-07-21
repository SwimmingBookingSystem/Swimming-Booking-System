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
using System.Threading.Tasks;

namespace SBS.WebApp.Pages.Customer.Profile;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public IndexModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [BindProperty]
    [Required(ErrorMessage = "Vui lòng nhập họ tên")]
    public string FullName { get; set; } = null!;

    [BindProperty]
    public DateOnly? Dob { get; set; }

    [BindProperty]
    public string? Gender { get; set; }

    [BindProperty]
    public string? Address { get; set; }

    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(_configuration["ApiBaseUrl"] ?? "https://localhost:7179");
        
        // Đọc token từ Claims đã được giải mã bởi Cookie Authentication middleware
        var token = User.FindFirst("AccessToken")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        
        return client;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        if (!User.Identity?.IsAuthenticated == true || string.IsNullOrEmpty(User.FindFirst("AccessToken")?.Value))
        {
            return RedirectToPage("/Auth/Login");
        }

        var client = CreateClient();
        var response = await client.GetAsync("/api/profile");
        
        if (!response.IsSuccessStatusCode)
        {
            return RedirectToPage("/Auth/Login");
        }

        var profile = await response.Content.ReadFromJsonAsync<UserProfileDto>();
        if (profile == null)
        {
            return RedirectToPage("/Index");
        }

        FullName = profile.FullName;
        Dob = profile.Dob;
        Gender = profile.Gender;
        Address = profile.Address;
        Email = profile.Email;
        AvatarUrl = profile.AvatarUrl;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!User.Identity?.IsAuthenticated == true || string.IsNullOrEmpty(User.FindFirst("AccessToken")?.Value))
        {
            return RedirectToPage("/Auth/Login");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var client = CreateClient();
        var request = new UpdateProfileRequest
        {
            FullName = FullName,
            Dob = Dob,
            Gender = Gender,
            Address = Address
        };

        var response = await client.PutAsJsonAsync("/api/profile", request);
        
        if (response.IsSuccessStatusCode)
        {
            TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Đã có lỗi xảy ra khi cập nhật thông tin. Vui lòng kiểm tra lại.");
        }

        // Reload email and avatar
        var profileResponse = await client.GetAsync("/api/profile");
        if (profileResponse.IsSuccessStatusCode)
        {
            var profile = await profileResponse.Content.ReadFromJsonAsync<UserProfileDto>();
            if (profile != null)
            {
                Email = profile.Email;
                AvatarUrl = profile.AvatarUrl;
            }
        }

        return Page();
    }
}
