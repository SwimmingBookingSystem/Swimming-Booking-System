using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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
using System.Security.Claims;
using System.Threading.Tasks;

namespace SBS.WebApp.Pages.Customer.Profile;

public class SyncAvatarRequest
{
    public string AvatarUrl { get; set; } = null!;
}

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
        AvatarUrl = profile.EffectiveAvatarUrl;

        if (!string.IsNullOrEmpty(AvatarUrl))
        {
            await RefreshAvatarClaimAsync(AvatarUrl);
        }

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
                AvatarUrl = profile.EffectiveAvatarUrl;
                if (!string.IsNullOrEmpty(AvatarUrl))
                {
                    await RefreshAvatarClaimAsync(AvatarUrl);
                }
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostSyncAvatarClaimAsync([FromBody] SyncAvatarRequest request)
    {
        if (request != null && !string.IsNullOrEmpty(request.AvatarUrl))
        {
            await RefreshAvatarClaimAsync(request.AvatarUrl);
            return new JsonResult(new { success = true });
        }
        return new JsonResult(new { success = false });
    }

    private async Task RefreshAvatarClaimAsync(string newAvatarUrl)
    {
        if (User.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated) return;

        var existingClaim = identity.FindFirst("AvatarUrl");
        if (existingClaim?.Value == newAvatarUrl) return;

        if (existingClaim != null)
        {
            identity.RemoveClaim(existingClaim);
        }
        identity.AddClaim(new Claim("AvatarUrl", newAvatarUrl ?? ""));

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));
    }
}
