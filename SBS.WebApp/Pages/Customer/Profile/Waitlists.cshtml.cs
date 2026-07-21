using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using SBS.WebApp.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace SBS.WebApp.Pages.Customer.Profile;

public class WaitlistsModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public WaitlistsModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public List<CustomerWaitlistDto> Waitlists { get; set; } = new List<CustomerWaitlistDto>();

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
        var response = await client.GetAsync("/api/customer-bookings/waitlist/my-waitlists");
        
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<List<CustomerWaitlistDto>>();
            if (result != null)
            {
                Waitlists = result;
            }
        }
        return Page();
    }
}
