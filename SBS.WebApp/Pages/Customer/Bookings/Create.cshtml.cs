using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace SBS.WebApp.Pages.Customer.Bookings
{
    public class CreateModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public CreateModel(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty(SupportsGet = true)]
        public int PoolId { get; set; }

        public string ApiBaseUrl { get; private set; } = null!;
        public PoolInfoDto PoolInfo { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync()
        {
            if (PoolId <= 0)
            {
                return RedirectToPage("/CustomerViewPoolIst/CustomerViewPoolIst");
            }

            ApiBaseUrl = _configuration["ApiBaseUrl"] ?? "https://localhost:7179";

            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{ApiBaseUrl}/api/customer/pools/{PoolId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                PoolInfo = JsonSerializer.Deserialize<PoolInfoDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
                
                if (PoolInfo == null)
                {
                    return RedirectToPage("/CustomerViewPoolIst/CustomerViewPoolIst");
                }
            }
            else
            {
                return RedirectToPage("/CustomerViewPoolIst/CustomerViewPoolIst");
            }

            return Page();
        }

        public class PoolInfoDto
        {
            public int PoolId { get; set; }
            public string PoolName { get; set; } = null!;
            public string Address { get; set; } = null!;
            public string? Description { get; set; }
            public string[] Images { get; set; } = System.Array.Empty<string>();
        }
    }
}
