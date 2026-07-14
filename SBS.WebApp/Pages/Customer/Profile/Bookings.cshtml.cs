using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using SBS.WebApp.Models.Profile;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace SBS.WebApp.Pages.Customer.Profile;

[Authorize(Roles = "Customer")]
public class BookingsModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public BookingsModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public List<CustomerBookingHistoryDto> Bookings { get; set; } = new List<CustomerBookingHistoryDto>();

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

    public async Task OnGetAsync()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/customer-bookings/history");
        
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<List<CustomerBookingHistoryDto>>();
            if (result != null)
            {
                Bookings = result;
            }
        }
    }
}
