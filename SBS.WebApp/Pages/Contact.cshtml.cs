using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using SBS.WebApp.Models.Profile;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SBS.WebApp.Pages;

public class ContactModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public ContactModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public string PreFillFullName { get; set; } = "";
    public string PreFillEmail { get; set; } = "";
    public string PreFillPhone { get; set; } = "";

    public async Task OnGetAsync()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        PreFillFullName = User.FindFirst(ClaimTypes.Name)?.Value ?? "";
        PreFillEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
        PreFillPhone = User.FindFirst(ClaimTypes.MobilePhone)?.Value ?? "";

        var accessToken = User.FindFirst("AccessToken")?.Value;
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return;
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_configuration["ApiBaseUrl"] ?? "https://localhost:7179");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetAsync("/api/profile");
            if (!response.IsSuccessStatusCode)
            {
                return;
            }

            var profile = await response.Content.ReadFromJsonAsync<UserProfileDto>();
            if (profile is null)
            {
                return;
            }

            PreFillFullName = string.IsNullOrWhiteSpace(profile.FullName) ? PreFillFullName : profile.FullName;
            PreFillEmail = string.IsNullOrWhiteSpace(profile.Email) ? PreFillEmail : profile.Email;
        }
        catch (HttpRequestException)
        {
            // The contact page remains available even if profile prefill is temporarily unavailable.
        }
    }
}